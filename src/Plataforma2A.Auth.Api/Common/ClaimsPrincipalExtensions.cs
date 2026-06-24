using System.Security.Claims;

namespace Plataforma2A.Auth.Api.Common;

/// <summary>Helpers para extrair dados do usuário autenticado a partir das claims do JWT.</summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>Id do usuário a partir da claim <c>sub</c> (ou <c>NameIdentifier</c>), ou null se ausente/inválido.</summary>
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(raw, out var id) ? id : null;
    }
}
