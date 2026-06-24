namespace Plataforma2A.Auth.Infrastructure.Authentication;

/// <summary>Configuração do JWT e dos tokens (seção Jwt). Segredo via env/secret store.</summary>
public sealed class JwtOptions
{
    public string Issuer { get; set; } = "Plataforma2A.Auth";
    public string Audience { get; set; } = "Plataforma2A.Auth";
    public string Key { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
