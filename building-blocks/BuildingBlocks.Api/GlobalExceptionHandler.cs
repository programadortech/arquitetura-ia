using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Api;

/// <summary>
/// Captura exceções não tratadas (inesperadas) e responde no envelope, sem vazar stack/PII.
/// Falhas de negócio NÃO chegam aqui — retornam Result/envelope (ver docs/standards/error-handling.md).
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        logger.LogError(exception, "Erro inesperado. TraceId {TraceId}", traceId);

        var response = ApiResponse<object?>.Fail(
            [new ApiError("erro.inesperado", "Ocorreu um erro inesperado.", "Internal")],
            traceId);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }
}
