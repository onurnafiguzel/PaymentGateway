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
using Shared.Messaging.Events.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder
    .Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// EventBus
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

// Subscribers
builder.Services.AddSingleton<PaymentCreatedSubscriber>();
builder.Services.AddSingleton<FraudCheckSubscriber>();
//builder.Services.AddSingleton<ProviderPaymentSubscriber>();



builder.Services.AddSingleton<StripeProviderClient>();
builder.Services.AddSingleton<PayTrProviderClient>();
builder.Services.AddSingleton<IyzicoProviderClient>();

builder.Services.AddSingleton<IProviderSelector, MockProviderSelector>();
builder.Services.AddHttpClient<IPaymentInitiator, PaymentInitiator>();

// Repository, UoW vs (Scopeds)
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

// SUBSCRIBERS MUST BE RESOLVED MANUALLY
_ = app.Services.GetRequiredService<PaymentCreatedSubscriber>();
_ = app.Services.GetRequiredService<FraudCheckSubscriber>();
//_ = app.Services.GetRequiredService<ProviderPaymentSubscriber>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
