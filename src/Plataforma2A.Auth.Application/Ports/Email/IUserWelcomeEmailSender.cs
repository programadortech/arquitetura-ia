namespace Plataforma2A.Auth.Application.Ports.Email;

/// <summary>
/// Porta de envio do e-mail de boas-vindas (login + senha temporária). AZ-12114.
/// O envio real é história futura — o adapter atual é um stub (ADR-0026).
/// </summary>
public interface IUserWelcomeEmailSender
{
    Task SendWelcomeEmailAsync(string email, string userName, string temporaryPassword, CancellationToken cancellationToken);
}
