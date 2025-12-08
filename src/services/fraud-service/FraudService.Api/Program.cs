using FraudService.Api.Subscriber;
using PaymentOrchestrator.Infrastructure.Messaging;
using Shared.Messaging.Events.Common;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<FraudPaymentSubscriber>();
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

var app = builder.Build();

_ = app.Services.GetRequiredService<FraudPaymentSubscriber>();

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
