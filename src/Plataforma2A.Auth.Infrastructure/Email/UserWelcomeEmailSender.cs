using Microsoft.Extensions.Logging;
using Plataforma2A.Auth.Application.Ports.Email;

namespace Plataforma2A.Auth.Infrastructure.Email;

/// <summary>
/// Stub do e-mail de boas-vindas (AZ-12114 / ADR-0026). O envio real é história futura.
/// Apenas registra que o envio foi preparado — NUNCA loga a senha temporária.
/// </summary>
public sealed class UserWelcomeEmailSender(ILogger<UserWelcomeEmailSender> logger) : IUserWelcomeEmailSender
{
    public Task SendWelcomeEmailAsync(string email, string userName, string temporaryPassword, CancellationToken cancellationToken)
    {
        // TODO: implementar o envio real de e-mail em história futura (ex.: via IEmailSender / provedor do catálogo).
        // Não registrar temporaryPassword em log.
        logger.LogInformation("E-mail de boas-vindas preparado para o usuário {UserName}", userName);
        return Task.CompletedTask;
    }
}
