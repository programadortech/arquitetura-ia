using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Plataforma2A.Auth.Api.Common;
using Plataforma2A.Auth.Infrastructure.Authentication;
using Serilog;

namespace Plataforma2A.Auth.Api.Extensions;

/// <summary>
/// Composição da camada Api por preocupação (ADR-0028 / docs/standards/api-layer.md).
/// Cada método registra UMA preocupação e é encadeável — mantém o Program.cs enxuto.
/// </summary>
public static class ServiceCollectionExtensions
{
    private const string ServiceName = "Plataforma2A.Auth.Api";

    /// <summary>Logs estruturados (Serilog) + telemetria (OpenTelemetry → OTLP).</summary>
    public static IServiceCollection AddObservability(this IServiceCollection services)
    {
        services.AddSerilog(cfg => cfg.Enrich.FromLogContext().WriteTo.Console());
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(ServiceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter());
        return services;
    }

    /// <summary>Controllers + ProblemDetails + middleware global de exceções + health checks.</summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddHealthChecks();
        return services;
    }

    /// <summary>Documentação de API (OpenAPI nativo + Scalar/Swagger na pipeline).</summary>
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi();
        return services;
    }

    /// <summary>Autenticação JWT Bearer a partir do JwtOptions único do DI (claim 'sub' preservada).</summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<JwtOptions>((options, jwt) =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ClockSkew = TimeSpan.Zero,
                };
            });
        return services;
    }

    /// <summary>Policies de autorização (ex.: administração de usuários — AZ-12114 / ADR-0026).</summary>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
            options.AddPolicy("users:manage", policy => policy.RequireRole("Administrador")));
        return services;
    }

    /// <summary>Rate limiting por IP para os endpoints públicos sensíveis (login/forgot/reset — AC #11).</summary>
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("auth", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    }));
        });
        return services;
    }
}
