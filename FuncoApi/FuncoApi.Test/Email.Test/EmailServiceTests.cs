using FuncoApi.Email;
using FuncoApi.Email.service;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FuncoApi.Test.Email.Test;

/// <summary>Comprueba el contenido y la entrega de mensajes construidos por EmailService.</summary>
public class EmailServiceTests
{
    private static readonly EmailSettings Settings = new()
    {
        Host = "smtp.gmail.com",
        Port = 587,
        SenderEmail = "funko@example.com",
        SenderName = "Funko API",
        Password = "test-app-password"
    };

    /// <summary>Comprueba que el correo individual tiene remitente, destino, asunto y cuerpo correctos.</summary>
    [Fact]
    public async Task SendEmailAsync_UsesConfiguredSenderAndRecipient()
    {
        var transport = new RecordingEmailTransport();
        var service = CreateService(transport);

        await service.SendEmailAsync("ana@example.com", "Asunto", "Contenido");

        var message = Assert.IsType<MimeMessage>(transport.Message);
        Assert.Equal("funko@example.com", Assert.Single(message.From.Mailboxes).Address);
        Assert.Equal("ana@example.com", Assert.Single(message.To.Mailboxes).Address);
        Assert.Equal("Asunto", message.Subject);
        Assert.Equal("Contenido", Assert.IsType<TextPart>(message.Body).Text);
        Assert.Equal(Settings, transport.Settings);
    }

    /// <summary>Comprueba que los destinatarios masivos quedan ocultos y se deduplican.</summary>
    [Fact]
    public async Task SendEmailAsync_ForManyRecipients_UsesBccAndRemovesDuplicates()
    {
        var transport = new RecordingEmailTransport();
        var service = CreateService(transport);

        await service.SendEmailAsync(
            new[] { "ana@example.com", "luis@example.com", "ANA@example.com", "" },
            "Nuevo Funko",
            "Se ha añadido un Funko.");

        var message = Assert.IsType<MimeMessage>(transport.Message);
        Assert.Empty(message.To.Mailboxes);
        Assert.Equal(
            new[] { "ana@example.com", "luis@example.com" },
            message.Bcc.Mailboxes.Select(mailbox => mailbox.Address));
        Assert.Equal("Nuevo Funko", message.Subject);
        Assert.Equal("Se ha añadido un Funko.", Assert.IsType<TextPart>(message.Body).Text);
    }

    /// <summary>Comprueba que una lista vacía no intenta abrir una conexión SMTP.</summary>
    [Fact]
    public async Task SendEmailAsync_ForNoRecipients_DoesNotSend()
    {
        var transport = new RecordingEmailTransport();
        var service = CreateService(transport);

        await service.SendEmailAsync(Array.Empty<string>(), "Asunto", "Contenido");

        Assert.Null(transport.Message);
    }

    private static EmailService CreateService(IEmailTransport transport) =>
        new(Options.Create(Settings), transport);

    private sealed class RecordingEmailTransport : IEmailTransport
    {
        public MimeMessage? Message { get; private set; }

        public EmailSettings? Settings { get; private set; }

        public Task SendAsync(MimeMessage message, EmailSettings settings)
        {
            Message = message;
            Settings = settings;
            return Task.CompletedTask;
        }
    }
}
