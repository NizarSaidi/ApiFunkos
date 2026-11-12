namespace FuncoApi.Dto.UserDto;

/// <summary>Credenciales enviadas para iniciar sesión.</summary>
public class LogginDto
{
    /// <summary>Correo electrónico de la cuenta.</summary>
    public string Email { get; set; }

    /// <summary>Contraseña en texto claro que se validará.</summary>
    public string Password { get; set; }
}