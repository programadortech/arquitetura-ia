using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Plataforma2A.Auth.Application.Abstractions;
using Plataforma2A.Auth.Application.Behaviors;

namespace Plataforma2A.Auth.Application;

/// <summary>DI da camada Application: dispatcher, behaviors e handlers de use case.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies is null || assemblies.Length == 0)
        {
            assemblies = [typeof(DependencyInjection).Assembly];
        }

        services.AddScoped<IUseCaseDispatcher, UseCaseDispatcher>();
        services.AddScoped(typeof(IUseCaseBehavior<,>), typeof(LoggingBehavior<,>));

        foreach (var assembly in assemblies)
        {
            var handlers = assembly.GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false })
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IUseCase<,>))
                    .Select(i => (Service: i, Implementation: t)));

            foreach (var (service, implementation) in handlers)
            {
                services.AddScoped(service, implementation);
            }
        }

        return services;
    }
}
