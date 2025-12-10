using MassTransit;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PaymentOrchestrator.Application;
using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Payments.Consumers;
using PaymentOrchestrator.Application.Payments.Services;
using PaymentOrchestrator.Application.Persistence;
using PaymentOrchestrator.Infrastructure;
using PaymentOrchestrator.Infrastructure.Persistence;
using PaymentOrchestrator.Infrastructure.Providers;
using PaymentOrchestrator.Infrastructure.Repositories;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Shared.Kernel;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------
// Serilog Logging Configuration
// -------------------------------
ConfigureLogging();
builder.Host.UseSerilog();

// -------------------------------
// Services
// -------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Application – Domain – Infrastructure
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);


// -------------------------------
// Provider Clients
// -------------------------------
builder.Services.AddSingleton<StripeProviderClient>();
builder.Services.AddSingleton<PayTrProviderClient>();
builder.Services.AddSingleton<IyzicoProviderClient>();

builder.Services.AddSingleton<IProviderSelector, MockProviderSelector>();
builder.Services.AddHttpClient<IPaymentInitiator, PaymentInitiator>();

// -------------------------------
// Database & Unit of Work
// -------------------------------
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// -------------------------------
// Middleware (Correlation Id)
// -------------------------------
builder.Services.AddTransient<CorrelationIdMiddleware>();

// -------------------------------
// OPEN TELEMETRY TRACING
// -------------------------------
builder.Services.AddOpenTelemetry()
    .WithTracing(tracer =>
    {
        tracer
            .AddAspNetCoreInstrumentation(o => o.RecordException = true)
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation(options =>
            {
                options.SetDbStatementForText = true;
                options.SetDbStatementForStoredProcedure = true;
            })
            // EVENT BUS Traces — KEY PART
            .AddSource("PaymentOrchestrator")

            // OTLP Exporter → Jaeger Collector
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService("payment-orchestrator-api")
            )
            .AddOtlpExporter(opt =>
            {
                opt.Endpoint = new Uri("http://localhost:4317"); // jeager-collector
            });
    });

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentCreatedConsumer>();
    x.AddConsumer<FraudCheckCompletedConsumer>();
    x.AddConsumer<ProviderInitiationConsumer>();
    x.AddConsumer<PaymentCompletedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("payment-created-queue", e =>
        {
            e.ConfigureConsumer<PaymentCreatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("fraud-completed-queue", e =>
        {
            e.ConfigureConsumer<FraudCheckCompletedConsumer>(context);
        });

        cfg.ReceiveEndpoint("provider-initiation-queue", e =>
        {
            e.ConfigureConsumer<ProviderInitiationConsumer>(context);
        });

        cfg.ReceiveEndpoint("payment-completed-queue", e =>
        {
            e.ConfigureConsumer<PaymentCompletedConsumer>(context);
        });
    });
});

var app = builder.Build();

// -------------------------------
// MIDDLEWARE PIPELINE
// -------------------------------
app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();


void ConfigureLogging()
{
    Log.Logger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .Enrich.WithCorrelationId()
        .WriteTo.Console()
        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
        {
            AutoRegisterTemplate = true,
            IndexFormat = $"payment-logs-{DateTime.UtcNow:yyyy-MM}",
            NumberOfReplicas = 1,
            NumberOfShards = 2
        })
        .CreateLogger();
}