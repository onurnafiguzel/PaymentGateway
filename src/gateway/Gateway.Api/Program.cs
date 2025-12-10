using Gateway.Api.Services;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Shared.Kernel;

var builder = WebApplication.CreateBuilder(args);

// --------------------------
// 1. Serilog Setup
// --------------------------

ConfigureLogging();
builder.Host.UseSerilog();

// --------------------------
// 2. OpenTelemetry Setup
// --------------------------
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerBuilder =>
    {
        tracerBuilder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://localhost:4317");
            })
            .AddSource("Gateway.Api"); // custom spans
    });


// --------------------------
// 3. CorrelationId Middleware
// --------------------------
builder.Services.AddTransient<CorrelationIdMiddleware>();

//builder.Services.AddHttpClient("gateway-client")
//    .AddOpenTelemetry();

builder.Services.AddHttpClient<IPaymentOrchestratorClient, PaymentOrchestratorClient>(c =>
{
    c.BaseAddress = new Uri("http://localhost:5001/");
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

Log.Information("TEST LOG FROM {Service}", "GatewayApi");

app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
            IndexFormat = $"gateway-logs-{DateTime.UtcNow:yyyy-MM}",
            NumberOfReplicas = 1,
            NumberOfShards = 2
        })
        .CreateLogger();
}
