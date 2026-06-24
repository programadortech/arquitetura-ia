using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Plataforma2A.Auth.Api.Common;
using Plataforma2A.Auth.Api.Endpoints;
using Plataforma2A.Auth.Application;
using Plataforma2A.Auth.Infrastructure;
using Plataforma2A.Auth.Infrastructure.Authentication;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Logs estruturados (Serilog).
builder.Services.AddSerilog(cfg => cfg.Enrich.FromLogContext().WriteTo.Console());

// Camadas (composition root).
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Autenticação JWT Bearer (claims preservadas: 'sub' não é remapeado).
// Configurado a partir do JwtOptions único registrado em AddInfrastructure (fonte de verdade — sem duplicar a leitura).
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services
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
builder.Services.AddAuthorization();

// Rate limiting (proteção contra força bruta — AC #11): janela fixa 10/min POR IP → 429.
builder.Services.AddRateLimiter(options =>
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

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

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

// Endpoints de autenticação e gerenciamento de senha (AZ-12094).
app.MapAuthEndpoints();

app.Run();

/// <summary>Exposto para os testes de integração (WebApplicationFactory).</summary>
public partial class Program;
