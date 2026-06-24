using System.Net.Mail;
using Plataforma2A.Auth.Application.Ports.Email;

namespace Plataforma2A.Auth.Infrastructure.Email;

/// <summary>Opções de SMTP (seção Smtp).</summary>
public sealed class SmtpOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 25;
    public string From { get; set; } = "no-reply@plataforma2a.local";
}

/// <summary>Envio de e-mail via SMTP (dev). Adapter da porta <see cref="IEmailSender"/>.</summary>
public sealed class SmtpEmailSender(SmtpOptions options) : IEmailSender
{
    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken)
    {
        using var client = new SmtpClient(options.Host, options.Port);
        using var message = new MailMessage(options.From, to, subject, body);
        await client.SendMailAsync(message, cancellationToken);
    }
}
