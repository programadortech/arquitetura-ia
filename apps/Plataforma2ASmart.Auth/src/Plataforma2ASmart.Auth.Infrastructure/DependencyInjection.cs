using BuildingBlocks.Application.Ports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Plataforma2ASmart.Auth.Infrastructure.Persistence;
using Polly;
using Polly.Retry;

namespace Plataforma2ASmart.Auth.Infrastructure;

/// <summary>DI da Infrastructure: banco (EF Core/SQL Server + UoW) e resiliência (Polly).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddResiliencePipeline("database", builder =>
            builder
                .AddTimeout(TimeSpan.FromSeconds(10))
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                }));

        return services;
    }
}
