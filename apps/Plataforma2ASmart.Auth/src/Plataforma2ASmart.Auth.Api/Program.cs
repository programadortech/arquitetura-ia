using Plataforma2ASmart.Auth.Api.Extensions;
using Plataforma2ASmart.Auth.Application;
using Plataforma2ASmart.Auth.Infrastructure;

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

app.UseApiPipeline();
app.MapApiDocumentation();
app.MapApiEndpoints();

app.Run();

/// <summary>Exposto para os testes de integração (WebApplicationFactory).</summary>
public partial class Program;
