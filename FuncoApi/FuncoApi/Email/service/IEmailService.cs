namespace FuncoApi.Email.service;

/// <summary>Define operaciones para enviar correos electrónicos.</summary>
public interface IEmailService
{
    /// <summary>Envía un correo a un destinatario.</summary>
    /// <param name="to">Dirección del destinatario.</param>
    /// <param name="subject">Asunto del mensaje.</param>
    /// <param name="body">Contenido de texto plano.</param>
    Task SendEmailAsync(string to, string subject, string body);

    /// <summary>Envía un correo con destinatarios ocultos entre sí.</summary>
    /// <param name="recipients">Direcciones que recibirán el mensaje en copia oculta.</param>
    /// <param name="subject">Asunto del mensaje.</param>
    /// <param name="body">Contenido de texto plano.</param>
    Task SendEmailAsync(IReadOnlyCollection<string> recipients, string subject, string body);
}