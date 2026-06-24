namespace Plataforma2A.Auth.Application.Ports.Email;

/// <summary>Porta de envio de e-mail (SMTP em dev). Provedor real plugável pelo catálogo docs/integrations/email.</summary>
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken);
}
