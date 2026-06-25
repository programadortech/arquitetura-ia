using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Plataforma2ASmart.Auth.Infrastructure.Identity;

/// <summary>Garante que as roles do sistema existam (idempotente). Roles não são segredo; ver AZ-12114 / ADR-P0002.</summary>
public static class IdentityRoleSeeder
{
    public static readonly string[] DefaultRoles = ["Administrador", "Operador", "Supervisor"];

    public static async Task SeedRolesAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        foreach (var role in DefaultRoles)
        {
            if (!await roles.RoleExistsAsync(role))
            {
                await roles.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }
}
