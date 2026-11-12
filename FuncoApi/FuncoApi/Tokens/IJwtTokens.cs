using FuncoApi.Models;

namespace FuncoApi.Tokens;

/// <summary>Define la generación y validación de tokens de acceso JWT.</summary>
public interface IJwtTokens
{
    /// <summary>
    /// Genera un token JWT para el usuario especificado.
    /// </summary>
    /// <param name="usuario">El usuario para el cual se generará el token.</param>
    /// <returns>El token JWT generado.</returns>
    string GenerateToken(User usuario);

    /// <summary>
    /// Valida un token JWT y devuelve el usuario correspondiente si es válido.
    /// </summary>
    /// <param name="token">El token JWT a validar.</param>
    /// <returns>El usuario correspondiente si el token es válido; de lo contrario, null.</returns>
    User? ValidateToken(string token);
}