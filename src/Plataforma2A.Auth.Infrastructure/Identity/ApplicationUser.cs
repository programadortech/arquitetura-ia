using Microsoft.AspNetCore.Identity;

namespace Plataforma2A.Auth.Infrastructure.Identity;

/// <summary>Usuário do ASP.NET Core Identity (chave Guid). Detalhe de Infrastructure.</summary>
public sealed class ApplicationUser : IdentityUser<Guid>;
