using Microsoft.AspNetCore.Identity;

namespace Plataforma2A.Auth.Infrastructure.Identity;

/// <summary>Usuário do ASP.NET Core Identity (chave Guid). Detalhe de Infrastructure.</summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>Nome completo do usuário (AZ-12114).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Status administrativo. Usuário inativo não autentica (ADR-0026).</summary>
    public bool IsActive { get; set; } = true;
}
