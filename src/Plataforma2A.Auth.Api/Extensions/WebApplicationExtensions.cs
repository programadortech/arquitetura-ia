using Scalar.AspNetCore;
using Serilog;

namespace Plataforma2A.Auth.Api.Extensions;

/// <summary>Pipeline e mapeamento da camada Api (ADR-0028 / docs/standards/api-layer.md). Mantém o Program.cs enxuto.</summary>
public static class WebApplicationExtensions
{
    /// <summary>Pipeline de requisição: exceções → logging → rate limit → autenticação/autorização.</summary>
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseSerilogRequestLogging();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    /// <summary>Documentação (fora de produção) + rota raiz utilitária (fora do OpenAPI).</summary>
    public static WebApplication MapApiDocumentation(this WebApplication app)
    {
        if (!app.Environment.IsProduction())
        {
            app.MapOpenApi();                              // documento: /openapi/v1.json
            app.MapScalarApiReference();                   // UI Scalar: /scalar
            app.UseSwaggerUI(options =>                    // UI Swagger: /swagger (lê o OpenAPI nativo)
            {
                options.SwaggerEndpoint("/openapi/v1.json", "Plataforma2A.Auth v1");
                options.RoutePrefix = "swagger";
            });
            app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();
        }
        else
        {
            app.MapGet("/", () => "Plataforma2A.Auth API · 2A Always Ahead").ExcludeFromDescription();
        }
        return app;
    }

    /// <summary>Mapeia os controllers e o health check.</summary>
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapControllers();
        app.MapHealthChecks("/health").ExcludeFromDescription();
        return app;
    }
}
