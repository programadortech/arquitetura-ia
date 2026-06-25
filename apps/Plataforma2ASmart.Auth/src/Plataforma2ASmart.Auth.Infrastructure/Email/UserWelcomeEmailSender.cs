using Microsoft.Extensions.Logging;
using Plataforma2ASmart.Auth.Application.Ports.Email;

namespace Plataforma2ASmart.Auth.Infrastructure.Email;

/// <summary>Implementação temporária do e-mail de boas-vindas (AZ-12114): apenas registra o preparo.</summary>
public sealed class UserWelcomeEmailSender(ILogger<UserWelcomeEmailSender> logger) : IUserWelcomeEmailSender
{
    public Task SendWelcomeEmailAsync(string email, string userName, string temporaryPassword, CancellationToken cancellationToken)
    {
        // TODO: implementar envio real do e-mail de boas-vindas em história futura.
        // NUNCA registrar temporaryPassword em log.
        logger.LogInformation("E-mail de boas-vindas preparado para o usuário {UserName}", userName);
        return Task.CompletedTask;
    }
}
