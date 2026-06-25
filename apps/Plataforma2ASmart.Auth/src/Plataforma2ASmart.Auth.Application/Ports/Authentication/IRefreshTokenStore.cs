namespace Plataforma2ASmart.Auth.Application.Ports.Authentication;

/// <summary>Refresh token emitido (valor em claro, entregue uma única vez) e sua expiração.</summary>
public sealed record RefreshTokenIssued(string Token, DateTimeOffset ExpiresAt);

/// <summary>
/// Porta de persistência de refresh tokens. Os métodos apenas rastreiam (Add/revoga) — o commit é do
/// caso de uso via IUnitOfWork (atomicidade da rotação).
/// </summary>
public interface IRefreshTokenStore
{
    Task<RefreshTokenIssued> IssueAsync(Guid userId, CancellationToken cancellationToken);
    Task<Guid?> ValidateAsync(string rawToken, CancellationToken cancellationToken);
    Task RevokeAsync(string rawToken, CancellationToken cancellationToken);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken);
}
