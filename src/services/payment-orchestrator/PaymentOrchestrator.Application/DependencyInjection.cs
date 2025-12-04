using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PaymentOrchestrator.Application.Payments.EventHandlers;
using PaymentOrchestrator.Infrastructure.Subscribers;
using System.Reflection;

namespace PaymentOrchestrator.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddSingleton<FraudCheckSubscriber>();
        services.AddSingleton<PaymentProviderSubscriber>();

        return services;
    }
}
