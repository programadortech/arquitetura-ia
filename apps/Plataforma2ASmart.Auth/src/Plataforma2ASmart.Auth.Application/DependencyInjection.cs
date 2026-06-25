using BuildingBlocks.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Plataforma2ASmart.Auth.Application;

/// <summary>DI da Application do produto: registra o dispatcher/behaviors e os handlers desta assembly.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
        => services.AddBuildingBlocksApplication(typeof(DependencyInjection).Assembly);
}
