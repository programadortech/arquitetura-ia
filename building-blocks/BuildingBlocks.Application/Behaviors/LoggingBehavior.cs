using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Application.Behaviors;

/// <summary>Loga início, sucesso e falha de cada caso de uso (logs estruturados).</summary>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IUseCaseBehavior<TRequest, TResponse>
    where TRequest : IUseCaseRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, UseCaseHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var useCase = typeof(TRequest).Name;
        logger.LogInformation("Executando use case {UseCase}", useCase);
        try
        {
            var response = await next().ConfigureAwait(false);
            logger.LogInformation("Use case {UseCase} concluído", useCase);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha no use case {UseCase}", useCase);
            throw;
        }
    }
}
