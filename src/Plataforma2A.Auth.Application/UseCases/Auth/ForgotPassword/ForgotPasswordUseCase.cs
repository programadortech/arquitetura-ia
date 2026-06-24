using Microsoft.Extensions.Logging;
using Plataforma2A.Auth.Application.Abstractions;
using Plataforma2A.Auth.Application.Common;
using Plataforma2A.Auth.Application.Ports.Authentication;
using Plataforma2A.Auth.Application.Ports.Email;

namespace Plataforma2A.Auth.Application.UseCases.Auth.ForgotPassword;

/// <summary>Solicita redefinição de senha (deslogado). Não revela se o e-mail existe. AC #8.</summary>
public sealed record ForgotPasswordRequest(string Email) : IUseCaseRequest<Result<Unit>>;

public sealed class ForgotPasswordHandler(
    IIdentityService identity,
    IEmailSender email,
    ILogger<ForgotPasswordHandler> logger) : IUseCase<ForgotPasswordRequest, Result<Unit>>
{
    public async Task<Result<Unit>> HandleAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await identity.FindByEmailAsync(request.Email, cancellationToken);
        if (user is not null)
        {
            var token = await identity.GeneratePasswordResetTokenAsync(user.UserId, cancellationToken);
            if (token is not null)
            {
                await email.SendAsync(
                    user.Email,
                    "Redefinição de senha",
                    $"Use o token a seguir para redefinir sua senha: {token}",
                    cancellationToken);
                logger.LogInformation("Token de redefinição enviado para {UserId}", user.UserId);
            }
        }

        // Resposta sempre genérica (não revela existência do e-mail).
        return Result<Unit>.Success(Unit.Value);
    }
}
