using Plataforma2ASmart.Auth.Domain.Common;

namespace Plataforma2ASmart.Auth.Domain.Authentication;

/// <summary>Refresh token persistido (sempre como hash). Entidade de domínio pura.</summary>
public sealed class RefreshToken : Entity<Guid>
{
    private RefreshToken() { TokenHash = string.Empty; } // EF

    public RefreshToken(Guid userId, string tokenHash, DateTimeOffset expiresAt, DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
    }

    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;
    public void Revoke(DateTimeOffset when) => RevokedAt ??= when;
}
