using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Common;
using BuildingBlocks.Application.Ports;
using Microsoft.Extensions.Logging;
using Plataforma2ASmart.Auth.Application.Ports.Authentication;

namespace Plataforma2ASmart.Auth.Application.UseCases.Auth.ResetPassword;

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

        // Lookup antes do reset garante que a revogação dos refresh tokens (AC #10) acompanhe o sucesso.
        // E-mail inexistente → sucesso genérico (não revela existência).
        var user = await identity.FindByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return Result<Unit>.Success(Unit.Value);
        }

        var ok = await identity.ResetPasswordAsync(request.Email, request.Token, request.NewPassword, cancellationToken);
        if (!ok)
        {
            logger.LogWarning("Redefinição recusada: token inválido ou expirado para {UserId}", user.UserId);
            return Result<Unit>.Failure(new Error("auth.token_reset_invalido", "Token de redefinição inválido ou expirado", ErrorType.Unauthorized));
        }

        await refreshTokens.RevokeAllForUserAsync(user.UserId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Senha redefinida e refresh tokens revogados para {UserId}", user.UserId);

        return Result<Unit>.Success(Unit.Value);
    }
}
