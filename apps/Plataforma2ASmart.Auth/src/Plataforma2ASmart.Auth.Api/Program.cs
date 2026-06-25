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
    .AddRateLimiting();

var app = builder.Build();

await app.Services.SeedRolesAsync();

app.UseApiPipeline();
app.MapApiDocumentation();
app.MapApiEndpoints();

app.Run();

/// <summary>Exposto para os testes de integração (WebApplicationFactory).</summary>
public partial class Program;
