using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentOrchestrator.Application.ReadModels.Payments.Abstractions;
using PaymentOrchestrator.Infrastructure.Persistence;
using PaymentOrchestrator.ReadModel.Persistence;

namespace PaymentOrchestrator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var witeCs = configuration.GetConnectionString("PaymentDb")
                               ?? throw new InvalidOperationException("Connection string 'PaymentDb' not found.");

        var readCs = configuration.GetConnectionString("ReadDatabase")
                     ?? throw new InvalidOperationException("Connection string 'ReadDatabase' not found.");

        services.AddDbContext<PaymentDbContext>(options =>
        {
            options.UseNpgsql(witeCs);
        });

        services.AddDbContext<PaymentReadDbContext>(options =>
        {
            options.UseNpgsql(readCs);
        });

        services.AddScoped<IPaymentReadDbContext>(
            sp => sp.GetRequiredService<PaymentReadDbContext>());

        return services;
    }
}
