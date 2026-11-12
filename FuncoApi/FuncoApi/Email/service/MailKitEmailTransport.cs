using FuncoApi.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace FuncoApi.Email.service;

/// <summary>Entrega mensajes MIME usando SMTP con STARTTLS mediante MailKit.</summary>
public sealed class MailKitEmailTransport : IEmailTransport
{
    /// <inheritdoc />
    public async Task SendAsync(MimeMessage message, EmailSettings settings)
    {
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(settings.Host, settings.Port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(settings.SenderEmail, settings.Password);
        await smtp.SendAsync(new SmtpSendRequest(message));
        await smtp.DisconnectAsync(true);
    }
}
