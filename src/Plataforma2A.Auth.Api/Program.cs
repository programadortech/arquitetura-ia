using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Plataforma2A.Auth.Api.Common;
using Plataforma2A.Auth.Application;
using Plataforma2A.Auth.Infrastructure;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Logs estruturados (Serilog).
builder.Services.AddSerilog(cfg => cfg.Enrich.FromLogContext().WriteTo.Console());

// Camadas (composition root).
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Documentação de API (OpenAPI nativo + Scalar + Swagger).
builder.Services.AddOpenApi();

// Telemetria (OpenTelemetry → OTLP).
const string serviceName = "Plataforma2A.Auth.Api";
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter());

// Middleware global de exceções + ProblemDetails (envelope para o inesperado).
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

// Docs só fora de produção (ver docs/standards/api-documentation.md).
if (!app.Environment.IsProduction())
{
    app.MapOpenApi();                          // documento: /openapi/v1.json
    app.MapScalarApiReference();               // UI Scalar: /scalar
    app.UseSwaggerUI(options =>                // UI Swagger: /swagger (lê o OpenAPI nativo)
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Plataforma2A.Auth v1");
        options.RoutePrefix = "swagger";
    });
    // base URL abre a documentação — utilitário, fora do OpenAPI.
    app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();
}
else
{
    app.MapGet("/", () => "Plataforma2A.Auth API · 2A Always Ahead").ExcludeFromDescription();
}

// Health check — não faz parte do contrato de API, fora do OpenAPI.
app.MapHealthChecks("/health").ExcludeFromDescription();

app.Run();

/// <summary>Exposto para os testes de integração (WebApplicationFactory).</summary>
public partial class Program;
