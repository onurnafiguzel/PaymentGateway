using MassTransit;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PaymentOrchestrator.Application;
using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Common.Observabilitiy;
using PaymentOrchestrator.Application.Payments.Consumers;
using PaymentOrchestrator.Application.Payments.Services;
using PaymentOrchestrator.Application.Persistence;
using PaymentOrchestrator.Application.Sagas.Payment;
using PaymentOrchestrator.Infrastructure;
using PaymentOrchestrator.Infrastructure.Messaging;
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
    })
    .WithMetrics(metrics =>
    {
        metrics
            // 🔥 Application katmanındaki Meter
            .AddMeter(PaymentMetrics.MeterName)

            // ASP.NET metrics (request duration vs.)
            .AddAspNetCoreInstrumentation()

            // HttpClient metrics
            .AddHttpClientInstrumentation()

            // İstersen EF Core metrics de eklenebilir
            //.AddRuntimeInstrumentation()

            // OTLP ile metrics export
            .AddOtlpExporter(opt =>
            {
                opt.Endpoint = new Uri("http://localhost:4317");
            })
            .AddPrometheusExporter();
    });

var connectionString = builder.Configuration.GetConnectionString("PaymentDb")
                   ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// MassTransit
builder.Services.AddMassTransit(x =>
{
    // ---------- CONSUMERS ----------
    x.AddConsumer<FraudCheckCompletedConsumer>();
    x.AddConsumer<ProviderInitiationConsumer>();
    x.AddConsumer<PaymentCompletedConsumer>();
    x.AddConsumer<PaymentCreatedConsumer>();
    //x.AddConsumer(typeof(FaultConsumer<>));


    // ---------- SAGA ----------
    x.AddSagaStateMachine<PaymentStateMachine, PaymentState>()
        .EntityFrameworkRepository(r =>
        {
            r.ConcurrencyMode = ConcurrencyMode.Pessimistic;

            r.AddDbContext<DbContext, PaymentDbContext>((provider, cfg) =>
            {
                cfg.UseNpgsql(builder.Configuration.GetConnectionString("PaymentDb"));
            });
        });

    // ---------- EF OUTBOX ----------
    x.AddEntityFrameworkOutbox<PaymentDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    // ---------- RETRY + DELAYED ----------
    x.AddConfigureEndpointsCallback((context, name, cfg) =>
    {
        cfg.UseDelayedRedelivery(r =>
        {
            r.Intervals(
                TimeSpan.FromSeconds(10),
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(5));
        });

        cfg.UseMessageRetry(r =>
        {
            r.Immediate(3);
        });
    });

    // ---------- RABBITMQ ----------
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("admin");
            h.Password("admin");
        });

        cfg.ConnectPublishObserver(
            context.GetRequiredService<CorrelationIdPublishObserver>());

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddSingleton<CorrelationIdPublishObserver>();


var app = builder.Build();

app.MapPrometheusScrapingEndpoint();
//app.UseOpenTelemetryPrometheusScrapingEndpoint();

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