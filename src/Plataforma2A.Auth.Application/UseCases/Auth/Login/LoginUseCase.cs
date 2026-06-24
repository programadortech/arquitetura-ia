using Plataforma2A.Auth.Application.Abstractions;
using Plataforma2A.Auth.Application.Common;
using Plataforma2A.Auth.Application.Ports.Authentication;

namespace Plataforma2A.Auth.Application.UseCases.Auth.Login;

/// <summary>Login por e-mail e senha. AC #1–#3.</summary>
public sealed record LoginRequest(string Email, string Password) : IUseCaseRequest<Result<AuthTokensResponse>>;

public sealed class LoginHandler(
    IIdentityService identity,
    IJwtTokenGenerator jwt,
    IRefreshTokenStore refreshTokens) : IUseCase<LoginRequest, Result<AuthTokensResponse>>
{
    public async Task<Result<AuthTokensResponse>> HandleAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await identity.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
        if (user is null)
        {
            // Mesma resposta para senha inválida e usuário inexistente (não revela existência).
            return Result<AuthTokensResponse>.Failure(
                new Error("auth.credenciais_invalidas", "Usuário ou senha inválidos", ErrorType.Unauthorized));
        }

        var access = jwt.Generate(user.UserId, user.Email, user.Roles);
        var refresh = await refreshTokens.IssueAsync(user.UserId, cancellationToken);
        return Result<AuthTokensResponse>.Success(new AuthTokensResponse(access.Token, refresh.Token, access.ExpiresAt));
    }
}
