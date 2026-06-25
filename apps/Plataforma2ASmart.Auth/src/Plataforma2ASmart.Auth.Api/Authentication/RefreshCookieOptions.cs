using Microsoft.AspNetCore.Http;

namespace Plataforma2ASmart.Auth.Api.Authentication;

/// <summary>Configuração do cookie httpOnly do refresh token (seção Auth:RefreshCookie). Ver ADR-P0003.</summary>
public sealed class RefreshCookieOptions
{
    public const string SectionName = "Auth:RefreshCookie";

    public string Name { get; set; } = "refresh_token";
    public string Path { get; set; } = "/api/auth";
    public bool Secure { get; set; } = true;
    public SameSiteMode SameSite { get; set; } = SameSiteMode.None;
    public int Days { get; set; } = 7;
}
