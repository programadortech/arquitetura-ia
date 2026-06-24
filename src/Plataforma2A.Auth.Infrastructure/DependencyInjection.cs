using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Plataforma2A.Auth.Application.Ports.Authentication;
using Plataforma2A.Auth.Application.Ports.Email;
using Plataforma2A.Auth.Application.Ports.Persistence;
using Plataforma2A.Auth.Infrastructure.Authentication;
using Plataforma2A.Auth.Infrastructure.Email;
using Plataforma2A.Auth.Infrastructure.Identity;
using Plataforma2A.Auth.Infrastructure.Persistence;
using Polly;
using Polly.Retry;

namespace Plataforma2A.Auth.Infrastructure;

/// <summary>DI da camada Infrastructure: banco (EF Core/SQL Server + UoW), Identity/JWT, e-mail e resiliência.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Banco: SQL Server (provider plugável) + Unit of Work (EF Core).
        var connectionString = configuration.GetConnectionString("Default");
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        // ASP.NET Core Identity (somente o core — sem cookies; a API usa JWT).
        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // Opções de JWT e SMTP (segredos via env/secret store — ADR-0022).
        var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
        var smtpOptions = configuration.GetSection("Smtp").Get<SmtpOptions>() ?? new SmtpOptions();
        services.AddSingleton(jwtOptions);
        services.AddSingleton(smtpOptions);

        // Ports de autenticação e e-mail.
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();
        services.AddSingleton<IEmailSender, SmtpEmailSender>();

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
