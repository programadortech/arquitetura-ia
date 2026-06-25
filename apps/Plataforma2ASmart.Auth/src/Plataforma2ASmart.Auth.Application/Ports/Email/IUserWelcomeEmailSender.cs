namespace Plataforma2ASmart.Auth.Application.Ports.Email;

/// <summary>
/// Porta do e-mail de boas-vindas (usuário criado sem senha → senha temporária). O envio real é de uma
/// história futura; a implementação atual apenas prepara/registra. Ver AZ-12114.
/// </summary>
public interface IUserWelcomeEmailSender
{
    Task SendWelcomeEmailAsync(string email, string userName, string temporaryPassword, CancellationToken cancellationToken);
}
