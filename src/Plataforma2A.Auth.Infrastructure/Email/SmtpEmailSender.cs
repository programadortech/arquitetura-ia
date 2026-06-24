using System.Net;
using System.Net.Mail;
using Plataforma2A.Auth.Application.Ports.Email;

namespace Plataforma2A.Auth.Infrastructure.Email;

/// <summary>Opções de SMTP (seção Smtp). Credenciais opcionais (provedor de produção do catálogo).</summary>
public sealed class SmtpOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 25;
    public string From { get; set; } = "no-reply@plataforma2a.local";
    public string? Username { get; set; }
    public string? Password { get; set; }
}

/// <summary>Envio de e-mail via SMTP (dev). Adapter da porta <see cref="IEmailSender"/>.</summary>
public sealed class SmtpEmailSender(SmtpOptions options) : IEmailSender
{
    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken)
    {
        // TODO: substituir por MailKit ao conectar provedor de produção (ver docs/integrations/email).
        // System.Net.Mail.SmtpClient é [Obsolete] no .NET 6+; mantido apenas como adapter de desenvolvimento.
        using var client = new SmtpClient(options.Host, options.Port);
        if (!string.IsNullOrEmpty(options.Username))
        {
            client.Credentials = new NetworkCredential(options.Username, options.Password);
        }
        using var message = new MailMessage(options.From, to, subject, body);
        await client.SendMailAsync(message, cancellationToken);
    }
}
