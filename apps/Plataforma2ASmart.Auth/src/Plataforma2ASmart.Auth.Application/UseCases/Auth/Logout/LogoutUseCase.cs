using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Common;
using BuildingBlocks.Application.Ports;
using Microsoft.Extensions.Logging;
using Plataforma2ASmart.Auth.Application.Ports.Authentication;

namespace Plataforma2ASmart.Auth.Application.UseCases.Auth.Logout;

/// <summary>Encerra a sessão: revoga o refresh token recebido. Idempotente (token ausente/inválido → sucesso).</summary>
public sealed record LogoutRequest(string RefreshToken) : IUseCaseRequest<Result<Unit>>;

public sealed class LogoutHandler(
    IRefreshTokenStore refreshTokens,
    IUnitOfWork unitOfWork,
    ILogger<LogoutHandler> logger) : IUseCase<LogoutRequest, Result<Unit>>
{
    public async Task<Result<Unit>> HandleAsync(LogoutRequest request, CancellationToken cancellationToken)
    {
        await refreshTokens.RevokeAsync(request.RefreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Logout: refresh token revogado");
        return Result<Unit>.Success(Unit.Value);
    }
}
