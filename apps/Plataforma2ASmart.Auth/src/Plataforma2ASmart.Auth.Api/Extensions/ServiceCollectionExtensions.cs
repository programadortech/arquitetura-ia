using System.Text;
using System.Threading.RateLimiting;
using BuildingBlocks.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Plataforma2ASmart.Auth.Api.Authentication;
using Plataforma2ASmart.Auth.Api.Authorization;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Plataforma2ASmart.Auth.Infrastructure.Authentication;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace Plataforma2ASmart.Auth.Api.Extensions;

/// <summary>Composição da camada Api por preocupação (ADR-0028). Mantém o Program.cs enxuto.</summary>
public static class ServiceCollectionExtensions
{
    private const string ServiceName = "Plataforma2ASmart.Auth.Api";
    public const string SpaCorsPolicy = "spa";

    /// <summary>CORS com credenciais para o SPA (front separado) + opções do cookie httpOnly do refresh. ADR-P0003.</summary>
    public static IServiceCollection AddSpaIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RefreshCookieOptions>(configuration.GetSection(RefreshCookieOptions.SectionName));

        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(options => options.AddPolicy(SpaCorsPolicy, policy =>
        {
            if (origins.Length > 0)
            {
                policy.WithOrigins(origins).AllowCredentials();
            }
            policy.AllowAnyHeader().AllowAnyMethod();
        }));
        return services;
    }

    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration)
    {
        var otlpEndpoint = configuration["OpenTelemetry:Otlp:Endpoint"];
        var hasOtlp = Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out var otlpUri);

        services.AddSerilog(cfg =>
        {
            cfg.Enrich.FromLogContext().WriteTo.Console();
            // Logs também via OTLP (aparecem no dashboard junto com traces/métricas) quando há endpoint — ADR-0033.
            if (hasOtlp)
            {
                cfg.WriteTo.OpenTelemetry(o =>
                {
                    o.Endpoint = otlpEndpoint!;
                    o.Protocol = OtlpProtocol.Grpc;
                    o.ResourceAttributes = new Dictionary<string, object> { ["service.name"] = ServiceName };
                });
            }
        });

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(ServiceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(o => { if (hasOtlp) { o.Endpoint = otlpUri!; } }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(o => { if (hasOtlp) { o.Endpoint = otlpUri!; } }));
        return services;
    }

    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddHealthChecks();
        return services;
    }

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
                    RoleClaimType = JwtOptions.RoleClaimType,
                };
            });
        services.AddScoped<IAuthorizationHandler, CreateUserAuthorizationHandler>();
        services.AddAuthorization(options =>
        {
            options.AddPolicy(UserPolicies.ManageUsers, policy => policy.RequireRole(UserPolicies.AdminRole));
            options.AddPolicy(UserPolicies.CreateUser, policy => policy.AddRequirements(new CreateUserRequirement()));
        });
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
