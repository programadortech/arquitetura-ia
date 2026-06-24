using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Plataforma2A.Auth.Application.Ports.Authentication;
using Plataforma2A.Auth.Application.Ports.Email;
using Plataforma2A.Auth.Application.Ports.Persistence;
using Plataforma2A.Auth.Application.Ports.Users;
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
        var connectionString = configuration.GetConnectionString("Default");
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        // Só o core do Identity: a API autentica por JWT, sem cookies.
        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
        var smtpOptions = configuration.GetSection("Smtp").Get<SmtpOptions>() ?? new SmtpOptions();

        // Fail-fast: chave ausente/curta só quebraria no primeiro login (HMAC-SHA256 exige ≥ 32 bytes).
        if (string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Key não configurado ou com comprimento insuficiente (mínimo 32 caracteres para HMAC-SHA256).");
        }

        services.AddSingleton(jwtOptions);
        services.AddSingleton(smtpOptions);

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IUserAdminService, UserAdminService>();
        services.AddScoped<IUserWelcomeEmailSender, UserWelcomeEmailSender>();

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
