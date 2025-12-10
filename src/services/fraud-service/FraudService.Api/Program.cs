using FraudService.Application.Consumers;
using MassTransit;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Shared.Kernel;

var builder = WebApplication.CreateBuilder(args);

ConfigureLogging();
builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<CorrelationIdMiddleware>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<FraudCheckRequestedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("fraud-check-requested-queue", e =>
        {
            e.ConfigureConsumer<FraudCheckRequestedConsumer>(context);
        });
    });
});

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();


// Configure the HTTP request pipeline.
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
            IndexFormat = $"fraud-logs-{DateTime.UtcNow:yyyy-MM}",
            NumberOfReplicas = 1,
            NumberOfShards = 2
        })
        .CreateLogger();
}