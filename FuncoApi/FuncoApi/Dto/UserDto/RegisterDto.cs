namespace FuncoApi.Dto.UserDto;

/// <summary>Datos requeridos para registrar una cuenta.</summary>
public class RegisterDto
{
    /// <summary>Nombre que se asignará a la cuenta.</summary>
    public string Name { get; set; }

    /// <summary>Correo electrónico de acceso.</summary>
    public string Email { get; set; }

    /// <summary>Contraseña en texto claro recibida para el registro.</summary>
    public string Password { get; set; }
}