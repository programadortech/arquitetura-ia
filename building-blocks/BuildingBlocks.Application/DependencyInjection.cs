using System.Reflection;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Application;

/// <summary>DI dos blocos da Application: dispatcher, behaviors e os handlers das assemblies do produto.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddBuildingBlocksApplication(this IServiceCollection services, params Assembly[] handlerAssemblies)
    {
        services.AddScoped<IUseCaseDispatcher, UseCaseDispatcher>();
        services.AddScoped(typeof(IUseCaseBehavior<,>), typeof(LoggingBehavior<,>));

        foreach (var assembly in handlerAssemblies)
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
