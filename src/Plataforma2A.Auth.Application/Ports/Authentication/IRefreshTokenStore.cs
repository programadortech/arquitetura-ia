namespace Plataforma2A.Auth.Application.Ports.Authentication;

/// <summary>Refresh token emitido (valor em claro, entregue uma única vez) e sua expiração.</summary>
public sealed record RefreshTokenIssued(string Token, DateTimeOffset ExpiresAt);

/// <summary>Porta de persistência de refresh tokens (geração, hash, expiração na Infrastructure).</summary>
public interface IRefreshTokenStore
{
    Task<RefreshTokenIssued> IssueAsync(Guid userId, CancellationToken cancellationToken);
    Task<Guid?> ValidateAsync(string rawToken, CancellationToken cancellationToken);
    Task RevokeAsync(string rawToken, CancellationToken cancellationToken);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken);
}
