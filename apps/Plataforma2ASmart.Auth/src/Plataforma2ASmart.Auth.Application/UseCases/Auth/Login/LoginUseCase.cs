using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Common;
using BuildingBlocks.Application.Ports;
using Microsoft.Extensions.Logging;
using Plataforma2ASmart.Auth.Application.Ports.Authentication;

namespace Plataforma2ASmart.Auth.Application.UseCases.Auth.Login;

/// <summary>Login por e-mail e senha. AC #1–#3.</summary>
public sealed record LoginRequest(string Email, string Password) : IUseCaseRequest<Result<AuthTokensResponse>>;

public sealed class LoginHandler(
    IIdentityService identity,
    IJwtTokenGenerator jwt,
    IRefreshTokenStore refreshTokens,
    IUnitOfWork unitOfWork,
    ILogger<LoginHandler> logger) : IUseCase<LoginRequest, Result<AuthTokensResponse>>
{
    public async Task<Result<AuthTokensResponse>> HandleAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await identity.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
        if (user is null)
        {
            logger.LogWarning("Login recusado por credenciais inválidas para {Email}", request.Email);
            return Result<AuthTokensResponse>.Failure(
                new Error("auth.credenciais_invalidas", "Usuário ou senha inválidos", ErrorType.Unauthorized));
        }

        var access = jwt.Generate(user.UserId, user.Email, user.Roles);
        var refresh = await refreshTokens.IssueAsync(user.UserId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Login bem-sucedido para {UserId}", user.UserId);
        return Result<AuthTokensResponse>.Success(new AuthTokensResponse(access.Token, refresh.Token, access.ExpiresAt));
    }
}
