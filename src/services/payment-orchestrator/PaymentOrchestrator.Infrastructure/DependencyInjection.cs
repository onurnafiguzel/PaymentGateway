using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Common.Events;
using PaymentOrchestrator.Application.Payments.EventHandlers;
using PaymentOrchestrator.Application.Payments.Services;
using PaymentOrchestrator.Application.Persistence;
using PaymentOrchestrator.Infrastructure.Persistence;
using PaymentOrchestrator.Infrastructure.Providers;
using PaymentOrchestrator.Infrastructure.Repositories;
using PaymentOrchestrator.Infrastructure.Subscribers;

namespace PaymentOrchestrator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PaymentDb")
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<PaymentDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        // Repository registration
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IEventBus, InMemoryEventBus>();

        services.AddSingleton<StripeProviderClient>();
        services.AddSingleton<PayTrProviderClient>();
        services.AddSingleton<IyzicoProviderClient>();

        services.AddSingleton<IProviderSelector, MockProviderSelector>();
        services.AddHttpClient<IPaymentInitiator, PaymentInitiator>();
        services.AddSingleton<PaymentCreatedSubscriber>();

        return services;
    }
}
