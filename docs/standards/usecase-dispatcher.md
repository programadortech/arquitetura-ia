# Padrão: Use Case Dispatcher (sem MediatR pago)

Usamos uma **abstração própria no repositório** para despachar casos de uso. **Não** dependemos de MediatR
(suas versões atuais são licenciadas comercialmente). Isso nos mantém livres de custo de licenciamento e nos dá
controle total sobre o pipeline.

## Contratos (camada Application)

```csharp
namespace <ProjectName>.Application.Abstractions;

/// <summary>A unit of application work: one request in, one response out.</summary>
public interface IUseCase<in TRequest, TResponse>
    where TRequest : IUseCaseRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}

/// <summary>Marker carrying the response type for a request.</summary>
public interface IUseCaseRequest<TResponse> { }

/// <summary>Resolves and invokes the handler for a request, running the behavior pipeline.</summary>
public interface IUseCaseDispatcher
{
    Task<TResponse> SendAsync<TResponse>(
        IUseCaseRequest<TResponse> request,
        CancellationToken cancellationToken = default);
}
```

## Pipeline behaviors (cross-cutting)

Um behavior envolve a execução do handler (logging, validação, transações, métricas):

```csharp
public interface IUseCaseBehavior<TRequest, TResponse>
    where TRequest : IUseCaseRequest<TResponse>
{
    Task<TResponse> HandleAsync(
        TRequest request,
        UseCaseHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}

public delegate Task<TResponse> UseCaseHandlerDelegate<TResponse>();
```

Behaviors embutidos recomendados, ordenados do mais externo → mais interno:
`LoggingBehavior` → `ValidationBehavior` → `TracingBehavior` → `TransactionBehavior`.

## Implementação do dispatcher (Infrastructure ou Application/Internal)

O dispatcher resolve o handler a partir da DI pelo tipo da request, compõe os behaviors registrados ao redor dele
e o invoca. A implementação usa `IServiceProvider` + um pequeno helper de resolução de genérico fechado
(sem reflection no caminho quente além da primeira chamada; faça cache do delegate invocador).

## Registro

```csharp
services.AddApplication();   // scans the Application assembly:
//  - registers all IUseCase<,> handlers
//  - registers IUseCaseDispatcher
//  - registers IUseCaseBehavior<,> in declared order
```

## Uso (endpoint da Api)

```csharp
app.MapPost("/orders/{id}/confirm", async (
    Guid id, IUseCaseDispatcher dispatcher, CancellationToken ct) =>
{
    var response = await dispatcher.SendAsync(new ConfirmOrderRequest(id), ct);
    return Results.Ok(response);
});
```

## Regras
- Handlers implementam `IUseCase<TRequest,TResponse>`; requests implementam `IUseCaseRequest<TResponse>`.
- Os chamadores dependem de `IUseCaseDispatcher`, nunca de um handler concreto.
- Preocupações cross-cutting vão em behaviors, não nos handlers.
- Um handler por tipo de request.
- Veja [ADR-0003](../adr/0003-usecase-dispatcher-no-mediatr.md).
