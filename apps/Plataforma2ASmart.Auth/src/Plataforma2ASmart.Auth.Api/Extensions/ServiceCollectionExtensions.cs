using BuildingBlocks.Api;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace Plataforma2ASmart.Auth.Api.Extensions;

/// <summary>Composição da camada Api por preocupação (ADR-0028). Mantém o Program.cs enxuto.</summary>
public static class ServiceCollectionExtensions
{
    private const string ServiceName = "Plataforma2ASmart.Auth.Api";

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
}
