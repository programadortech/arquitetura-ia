using Microsoft.AspNetCore.Identity;

namespace Plataforma2ASmart.Auth.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Inativo não autentica (gate no login). Inativar não remove o usuário. Ver AZ-12114.</summary>
    public bool IsActive { get; set; } = true;
}
