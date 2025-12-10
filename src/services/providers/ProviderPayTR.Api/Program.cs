using MassTransit;
using ProviderPayTR.Consumers;
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
        cfg.Host("localhost");

        cfg.ReceiveEndpoint("provider-initiation-queue", e =>
        {
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
