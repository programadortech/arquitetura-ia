namespace BuildingBlocks.Application.Abstractions;

/// <summary>Dispatcher próprio (sem MediatR): resolve o handler por tipo, compõe os behaviors e invoca.</summary>
public sealed class UseCaseDispatcher(IServiceProvider provider) : IUseCaseDispatcher
{
    public Task<TResponse> SendAsync<TResponse>(IUseCaseRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var handlerType = typeof(IUseCase<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = provider.GetService(handlerType)
            ?? throw new InvalidOperationException($"Nenhum handler registrado para {requestType.Name}.");
        var handleMethod = handlerType.GetMethod(nameof(IUseCase<IUseCaseRequest<TResponse>, TResponse>.HandleAsync))!;

        UseCaseHandlerDelegate<TResponse> next =
            () => (Task<TResponse>)handleMethod.Invoke(handler, [request, cancellationToken])!;

        var behaviorType = typeof(IUseCaseBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var behaviorMethod = behaviorType.GetMethod(nameof(IUseCaseBehavior<IUseCaseRequest<TResponse>, TResponse>.HandleAsync))!;
        var enumerableType = typeof(IEnumerable<>).MakeGenericType(behaviorType);
        var behaviors = ((IEnumerable<object>)(provider.GetService(enumerableType) ?? Array.Empty<object>()))
            .Reverse()
            .ToArray();

        foreach (var behavior in behaviors)
        {
            var current = next;
            next = () => (Task<TResponse>)behaviorMethod.Invoke(behavior, [request, current, cancellationToken])!;
        }

        return next();
    }
}
