namespace FuncoApi.Dto.UserDto;

/// <summary>Datos públicos devueltos tras registrar o autenticar a un usuario.</summary>
public class AutentificationResponseDto
{
    /// <summary>Token JWT para autenticar solicitudes posteriores.</summary>
    public string Token { get; set; }

    /// <summary>Nombre del usuario autenticado.</summary>
    public string Username { get; set; }

    /// <summary>Rol asignado al usuario.</summary>
    public string Rol { get; set; }
}