using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Plataforma2ASmart.Auth.Application.Ports.Authentication;
using Plataforma2ASmart.Auth.Domain.Authentication;
using Plataforma2ASmart.Auth.Infrastructure.Persistence;

namespace Plataforma2ASmart.Auth.Infrastructure.Authentication;

/// <summary>
/// Persiste refresh tokens como hash (SHA-256). Os métodos apenas rastreiam (Add/revoga) — o commit é do
/// caso de uso via IUnitOfWork, garantindo atomicidade da rotação.
/// </summary>
public sealed class RefreshTokenStore(AppDbContext db, JwtOptions options) : IRefreshTokenStore
{
    public Task<RefreshTokenIssued> IssueAsync(Guid userId, CancellationToken cancellationToken)
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddDays(options.RefreshTokenDays);

        db.RefreshTokens.Add(new RefreshToken(userId, Hash(raw), expires, now));
        return Task.FromResult(new RefreshTokenIssued(raw, expires));
    }

    public async Task<Guid?> ValidateAsync(string rawToken, CancellationToken cancellationToken)
    {
        var hash = Hash(rawToken);
        var token = await db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
        return token is not null && token.IsActive(DateTimeOffset.UtcNow) ? token.UserId : null;
    }

    public async Task RevokeAsync(string rawToken, CancellationToken cancellationToken)
    {
        var hash = Hash(rawToken);
        var token = await db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
        token?.Revoke(DateTimeOffset.UtcNow);
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var tokens = await db.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        foreach (var token in tokens)
        {
            token.Revoke(now);
        }
    }

    private static string Hash(string raw)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
}
