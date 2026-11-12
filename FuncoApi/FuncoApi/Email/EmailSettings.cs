namespace FuncoApi.Email;

/// <summary>Configuración SMTP para el envío de correo mediante Gmail.</summary>
public class EmailSettings
{
    /// <summary>Nombre DNS del servidor SMTP; Gmail usa <c>smtp.gmail.com</c>.</summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>Puerto STARTTLS del servidor SMTP; Gmail usa el puerto 587.</summary>
    public int Port { get; set; }

    /// <summary>Dirección de Gmail autenticada como remitente.</summary>
    public string SenderEmail { get; set; } = string.Empty;

    /// <summary>Nombre visible del remitente.</summary>
    public string SenderName { get; set; } = string.Empty;

    /// <summary>Contraseña de aplicación de Gmail; debe configurarse como secreto.</summary>
    public string Password { get; set; } = string.Empty;
}