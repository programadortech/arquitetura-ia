using Plataforma2ASmart.Auth.Api.Extensions;
using Plataforma2ASmart.Auth.Application;
using Plataforma2ASmart.Auth.Infrastructure;
using Plataforma2ASmart.Auth.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddObservability()
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApiServices()
    .AddApiDocumentation()
    .AddJwtAuthentication()
    .AddSpaIntegration(builder.Configuration)
    .AddRateLimiting();

var app = builder.Build();

// Durante a geração do contrato OpenAPI (build time, sem banco) NÃO semeamos — ver scripts/generate-openapi.ps1 (ADR-0032).
if (Environment.GetEnvironmentVariable("OPENAPI_GENERATION") != "true")
{
    // Seed resiliente: falha de banco não derruba o startup — só registra.
    try
    {
        await app.Services.SeedRolesAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Falha ao semear roles no startup; siga e semeie quando o banco estiver disponível.");
    }
}

app.UseApiPipeline();
app.MapApiDocumentation();
app.MapApiEndpoints();

app.Run();

/// <summary>Exposto para os testes de integração (WebApplicationFactory).</summary>
public partial class Program;
