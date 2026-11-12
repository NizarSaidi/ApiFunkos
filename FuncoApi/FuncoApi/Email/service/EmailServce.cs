using FuncoApi.Email;
using MimeKit;
using Microsoft.Extensions.Options;

namespace FuncoApi.Email.service;

/// <summary>Construye mensajes de correo y los entrega al transporte SMTP configurado.</summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly IEmailTransport _transport;

    /// <summary>Inicializa el servicio con la configuración de Gmail y el transporte SMTP.</summary>
    /// <param name="settings">Configuración del remitente y del servidor de correo.</param>
    /// <param name="transport">Transporte encargado de entregar el mensaje.</param>
    public EmailService(IOptions<EmailSettings> settings, IEmailTransport transport)
    {
        _settings = settings.Value;
        _transport = transport;
    }

    /// <inheritdoc />
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(to);
        var message = CreateMessage(subject, body);
        message.To.Add(MailboxAddress.Parse(to));
        await _transport.SendAsync(message, _settings);
    }

    /// <inheritdoc />
    public async Task SendEmailAsync(IReadOnlyCollection<string> recipients, string subject, string body)
    {
        ArgumentNullException.ThrowIfNull(recipients);
        var uniqueRecipients = recipients
            .Where(recipient => !string.IsNullOrWhiteSpace(recipient))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (uniqueRecipients.Length == 0)
        {
            return;
        }

        var message = CreateMessage(subject, body);
        foreach (var recipient in uniqueRecipients)
        {
            message.Bcc.Add(MailboxAddress.Parse(recipient));
        }

        await _transport.SendAsync(message, _settings);
    }

    private MimeMessage CreateMessage(string subject, string body)
    {
        ArgumentNullException.ThrowIfNull(subject);
        ArgumentNullException.ThrowIfNull(body);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };
        return message;
    }
}