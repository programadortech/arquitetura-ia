using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Plataforma2ASmart.Auth.Application.Ports.Authentication;

namespace Plataforma2ASmart.Auth.Infrastructure.Authentication;

/// <summary>Gera o Access Token JWT (HMAC-SHA256) com as claims mínimas do usuário.</summary>
public sealed class JwtTokenGenerator(JwtOptions options) : IJwtTokenGenerator
{
    public AccessToken Generate(Guid userId, string email, IReadOnlyCollection<string> roles)
    {
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Name, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        claims.AddRange(roles.Select(role => new Claim(JwtOptions.RoleClaimType, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: credentials);

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
