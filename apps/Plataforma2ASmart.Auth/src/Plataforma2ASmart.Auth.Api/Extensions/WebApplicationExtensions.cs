using Scalar.AspNetCore;
using Serilog;

namespace Plataforma2ASmart.Auth.Api.Extensions;

/// <summary>Pipeline e mapeamento da camada Api (ADR-0028). Mantém o Program.cs enxuto.</summary>
public static class WebApplicationExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseSerilogRequestLogging();
        app.UseCors(ServiceCollectionExtensions.SpaCorsPolicy);
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    public static WebApplication MapApiDocumentation(this WebApplication app)
    {
        if (!app.Environment.IsProduction())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "Plataforma2ASmart.Auth v1");
                options.RoutePrefix = "swagger";
            });
            app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();
        }
        else
        {
            app.MapGet("/", () => "Plataforma2ASmart.Auth API").ExcludeFromDescription();
        }
        return app;
    }

    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapControllers();
        app.MapHealthChecks("/health").ExcludeFromDescription();
        return app;
    }
}
