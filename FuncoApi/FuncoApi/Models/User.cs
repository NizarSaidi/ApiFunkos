namespace FuncoApi.Models;

/// <summary>Representa una cuenta de usuario; actualmente no está conectada a autenticación.</summary>
public class User
{
    /// <summary>Identificador del usuario.</summary>
    public int Id { get; set; }

    /// <summary>Nombre del usuario.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Dirección de correo electrónico del usuario.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Campo de contraseña del modelo; el modelo no define su almacenamiento seguro.</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Identificador del rol asignado al usuario.</summary>
    public int RolId { get; set; }
}