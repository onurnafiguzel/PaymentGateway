using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentOrchestrator.Infrastructure.Persistence;

namespace PaymentOrchestrator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PaymentDb")
                               ?? throw new InvalidOperationException("Connection string 'PaymentDb' not found.");

        services.AddDbContext<PaymentDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        return services;
    }
}
