using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PaymentOrchestrator.Api.Mİddlewares;
using PaymentOrchestrator.Application;
using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Payments.EventHandlers;
using PaymentOrchestrator.Application.Payments.Services;
using PaymentOrchestrator.Application.Persistence;
using PaymentOrchestrator.Infrastructure;
using PaymentOrchestrator.Infrastructure.Messaging;
using PaymentOrchestrator.Infrastructure.Persistence;
using PaymentOrchestrator.Infrastructure.Providers;
using PaymentOrchestrator.Infrastructure.Repositories;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Shared.Messaging.Events.Common;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------
// Serilog Logging Configuration
// -------------------------------
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationId()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "payment-orchestrator-logs"
    })
    .CreateLogger();

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
// EVENT BUS
// -------------------------------
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

// -------------------------------
// SUBSCRIBERS (Singleton)
// IMPORTANT: They attach handlers inside constructor
// -------------------------------
builder.Services.AddSingleton<PaymentCreatedSubscriber>();
builder.Services.AddSingleton<FraudCheckSubscriber>();
//builder.Services.AddSingleton<ProviderPaymentSubscriber>();

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
                opt.Endpoint = new Uri("http://localhost:4317");
            });
    });

var app = builder.Build();

// -------------------------------
// MIDDLEWARE PIPELINE
// -------------------------------
app.UseMiddleware<CorrelationIdMiddleware>();

// -------------------------------
// FORCE SUBSCRIBER INITIALIZATION
// -------------------------------
_ = app.Services.GetRequiredService<PaymentCreatedSubscriber>();
_ = app.Services.GetRequiredService<FraudCheckSubscriber>();
//_ = app.Services.GetRequiredService<ProviderPaymentSubscriber>();

// -------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
