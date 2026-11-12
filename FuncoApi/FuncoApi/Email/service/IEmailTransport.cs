using FuncoApi.Email;
using MimeKit;

namespace FuncoApi.Email.service;

/// <summary>Transporta mensajes MIME a través del servicio SMTP configurado.</summary>
public interface IEmailTransport
{
    /// <summary>Envía un mensaje mediante SMTP.</summary>
    /// <param name="message">Mensaje MIME completo.</param>
    /// <param name="settings">Configuración de conexión y credenciales SMTP.</param>
    Task SendAsync(MimeMessage message, EmailSettings settings);
}
