namespace Plataforma2ASmart.Auth.Infrastructure.Authentication;

/// <summary>Configuração do JWT (seção Jwt). Segredo via env/secret store (dev: user-secrets).</summary>
public sealed class JwtOptions
{
    public string Issuer { get; set; } = "Plataforma2ASmart.Auth";
    public string Audience { get; set; } = "Plataforma2ASmart.Auth";
    public string Key { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
