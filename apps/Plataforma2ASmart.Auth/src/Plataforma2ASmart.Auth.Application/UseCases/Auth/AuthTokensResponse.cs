namespace Plataforma2ASmart.Auth.Application.UseCases.Auth;

/// <summary>Par de tokens devolvido por login e refresh.</summary>
public sealed record AuthTokensResponse(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);
