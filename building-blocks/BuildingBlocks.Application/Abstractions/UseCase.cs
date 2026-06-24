namespace BuildingBlocks.Application.Abstractions;

/// <summary>Marca um request, carregando o tipo da resposta.</summary>
public interface IUseCaseRequest<TResponse>;

/// <summary>Unidade de trabalho da aplicação: um request entra, uma resposta sai.</summary>
public interface IUseCase<in TRequest, TResponse>
    where TRequest : IUseCaseRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}

/// <summary>Resolve e invoca o handler de um request, executando o pipeline de behaviors.</summary>
public interface IUseCaseDispatcher
{
    Task<TResponse> SendAsync<TResponse>(IUseCaseRequest<TResponse> request, CancellationToken cancellationToken = default);
}

/// <summary>Próximo passo do pipeline (o handler, no final).</summary>
public delegate Task<TResponse> UseCaseHandlerDelegate<TResponse>();

/// <summary>Behavior transversal que envolve a execução do handler.</summary>
public interface IUseCaseBehavior<in TRequest, TResponse>
    where TRequest : IUseCaseRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, UseCaseHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}
