using Plataforma2A.Auth.Api.Extensions;
using Plataforma2A.Auth.Application;
using Plataforma2A.Auth.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Composição por preocupação (ADR-0028 / docs/standards/api-layer.md). Cada extension faz uma coisa.
builder.Services
    .AddObservability()
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApiServices()
    .AddApiDocumentation()
    .AddJwtAuthentication()
    .AddAuthorizationPolicies()
    .AddRateLimiting();

var app = builder.Build();

app.UseApiPipeline();
app.MapApiDocumentation();
app.MapApiEndpoints();

app.Run();

/// <summary>Exposto para os testes de integração (WebApplicationFactory).</summary>
public partial class Program;
