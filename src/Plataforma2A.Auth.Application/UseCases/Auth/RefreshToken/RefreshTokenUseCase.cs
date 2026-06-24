using Plataforma2A.Auth.Application.Abstractions;
using Plataforma2A.Auth.Application.Common;
using Plataforma2A.Auth.Application.Ports.Authentication;

namespace Plataforma2A.Auth.Application.UseCases.Auth.RefreshToken;

/// <summary>Renova o Access Token com um Refresh Token válido, rotacionando-o. AC #4–#5.</summary>
public sealed record RefreshTokenRequest(string AccessToken, string RefreshToken)
    : IUseCaseRequest<Result<AuthTokensResponse>>;

public sealed class RefreshTokenHandler(
    IIdentityService identity,
    IJwtTokenGenerator jwt,
    IRefreshTokenStore refreshTokens) : IUseCase<RefreshTokenRequest, Result<AuthTokensResponse>>
{
    private static Error Invalid => new("auth.refresh_invalido", "Refresh token inválido ou expirado", ErrorType.Unauthorized);

    public async Task<Result<AuthTokensResponse>> HandleAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var userId = await refreshTokens.ValidateAsync(request.RefreshToken, cancellationToken);
        if (userId is null)
        {
            return Result<AuthTokensResponse>.Failure(Invalid);
        }

        var user = await identity.FindByIdAsync(userId.Value, cancellationToken);
        if (user is null)
        {
            return Result<AuthTokensResponse>.Failure(Invalid);
        }

        // Rotação: revoga o anterior e emite um novo.
        await refreshTokens.RevokeAsync(request.RefreshToken, cancellationToken);

        var access = jwt.Generate(user.UserId, user.Email, user.Roles);
        var refresh = await refreshTokens.IssueAsync(user.UserId, cancellationToken);
        return Result<AuthTokensResponse>.Success(new AuthTokensResponse(access.Token, refresh.Token, access.ExpiresAt));
    }
}
