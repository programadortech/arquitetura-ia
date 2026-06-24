namespace Plataforma2A.Auth.Application.Ports.Authentication;

/// <summary>Access Token emitido (JWT) e sua expiração.</summary>
public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);

/// <summary>Porta para geração do Access Token JWT (implementada na Infrastructure).</summary>
public interface IJwtTokenGenerator
{
    AccessToken Generate(Guid userId, string email, IReadOnlyCollection<string> roles);
}
