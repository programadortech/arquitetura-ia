using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Plataforma2A.Auth.Application.Ports.Persistence;
using Plataforma2A.Auth.Infrastructure.Persistence;
using Polly;
using Polly.Retry;

namespace Plataforma2A.Auth.Infrastructure;

/// <summary>DI da camada Infrastructure: banco (EF Core/SQL Server + UoW) e resiliência.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Banco: SQL Server (provider plugável) + Unit of Work (EF Core).
        var connectionString = configuration.GetConnectionString("Default");
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        // Resiliência: pipeline 'database' (timeout + retry com backoff e jitter).
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
