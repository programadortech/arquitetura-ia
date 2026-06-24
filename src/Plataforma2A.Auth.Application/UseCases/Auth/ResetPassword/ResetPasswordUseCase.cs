using Microsoft.Extensions.Logging;
using Plataforma2A.Auth.Application.Abstractions;
using Plataforma2A.Auth.Application.Common;
using Plataforma2A.Auth.Application.Ports.Authentication;
using Plataforma2A.Auth.Application.Ports.Persistence;

namespace Plataforma2A.Auth.Application.UseCases.Auth.ResetPassword;

/// <summary>Redefine a senha (deslogado) com token válido e invalida os refresh tokens. AC #9–#10.</summary>
public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword, string ConfirmNewPassword)
    : IUseCaseRequest<Result<Unit>>;

public sealed class ResetPasswordHandler(
    IIdentityService identity,
    IRefreshTokenStore refreshTokens,
    IUnitOfWork unitOfWork,
    ILogger<ResetPasswordHandler> logger) : IUseCase<ResetPasswordRequest, Result<Unit>>
{
    public async Task<Result<Unit>> HandleAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        if (request.NewPassword != request.ConfirmNewPassword)
        {
            return Result<Unit>.Failure(new Error("senha.confirmacao_invalida", "A nova senha e a confirmação não conferem", ErrorType.Validation));
        }

        var ok = await identity.ResetPasswordAsync(request.Email, request.Token, request.NewPassword, cancellationToken);
        if (!ok)
        {
            logger.LogWarning("Redefinição recusada: token inválido ou expirado para {Email}", request.Email);
            return Result<Unit>.Failure(new Error("auth.token_reset_invalido", "Token de redefinição inválido ou expirado", ErrorType.Unauthorized));
        }

        var user = await identity.FindByEmailAsync(request.Email, cancellationToken);
        if (user is not null)
        {
            await refreshTokens.RevokeAllForUserAsync(user.UserId, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Senha redefinida e refresh tokens revogados para {UserId}", user.UserId);
        }

        return Result<Unit>.Success(Unit.Value);
    }
}
