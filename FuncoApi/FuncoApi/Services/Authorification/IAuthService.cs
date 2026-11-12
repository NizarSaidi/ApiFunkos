using CSharpFunctionalExtensions;
using FuncoApi.Domain;
using FuncoApi.Dto.UserDto;

namespace FuncoApi.Services.Authorification;

/// <summary>Define los casos de uso de registro e inicio de sesión.</summary>
public interface IAuthService
{
    /// <summary>Registra una cuenta y genera un token JWT.</summary>
    /// <param name="dto">Datos necesarios para registrar al usuario.</param>
    /// <returns>La respuesta de autenticación o un error de dominio.</returns>
    Task<Result<AutentificationResponseDto, DomainError>> RegisterAsync(RegisterDto dto);

    /// <summary>Valida las credenciales y genera un token JWT.</summary>
    /// <param name="dto">Credenciales del usuario.</param>
    /// <returns>La respuesta de autenticación o un error de dominio.</returns>
    Task<Result<AutentificationResponseDto, DomainError>> LoginAsync(LogginDto dto);
}