using MassTransit;
using ProviderPayTR.Api.Consumers;
using Shared.Kernel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<CorrelationIdMiddleware>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProviderInitiationRequestedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("admin");
            h.Password("admin");
        });

        cfg.ReceiveEndpoint("provider-paytr-initiation-queue", e =>
        {
            // 🔁 Retry policy (transient hatalar için)
            e.UseMessageRetry(r =>
            {
                r.Exponential(
                    retryLimit: 5,
                    minInterval: TimeSpan.FromSeconds(1),
                    maxInterval: TimeSpan.FromSeconds(30),
                    intervalDelta: TimeSpan.FromSeconds(2));
            });

            // 🧠 Idempotent publish (double publish koruması)
            e.UseInMemoryOutbox();

            e.ConfigureConsumer<ProviderInitiationRequestedConsumer>(context);
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
