using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Plataforma2A.Auth.Application.Ports.Authentication;
using Plataforma2A.Auth.Domain.Authentication;
using Plataforma2A.Auth.Infrastructure.Persistence;

namespace Plataforma2A.Auth.Infrastructure.Authentication;

/// <summary>
/// Persiste refresh tokens como hash (SHA-256) no SQL Server; cuida de rotação/revogação.
/// Os métodos apenas **rastreiam** as mudanças (Add/revoga) — o commit é do caso de uso via
/// <see cref="Plataforma2A.Auth.Application.Ports.Persistence.IUnitOfWork"/>, garantindo atomicidade.
/// </summary>
public sealed class RefreshTokenStore(AppDbContext db, JwtOptions options) : IRefreshTokenStore
{
    public Task<RefreshTokenIssued> IssueAsync(Guid userId, CancellationToken cancellationToken)
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddDays(options.RefreshTokenDays);

        db.RefreshTokens.Add(new RefreshToken(userId, Hash(raw), expires, now));        return Task.FromResult(new RefreshTokenIssued(raw, expires));
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
        token?.Revoke(DateTimeOffset.UtcNow);    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var tokens = await db.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        foreach (var token in tokens)
        {
            token.Revoke(now);
        }    }

    private static string Hash(string raw)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
}
