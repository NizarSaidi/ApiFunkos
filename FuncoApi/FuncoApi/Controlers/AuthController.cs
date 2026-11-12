using FuncoApi.Dto.UserDto;
using FuncoApi.Services.Authorification;
using Microsoft.AspNetCore.Mvc;

namespace FuncoApi.Controlers;

/// <summary>Expone los endpoints HTTP de registro e inicio de sesión.</summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _service;

    /// <summary>Inicializa el controlador con el servicio de autenticación.</summary>
    /// <param name="service">Servicio que procesa el registro y el inicio de sesión.</param>
    public AuthController(IAuthService service)
    {
        _service = service;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AutentificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    /// <summary>Registra un usuario y devuelve sus credenciales de autenticación.</summary>
    /// <param name="dto">Datos del usuario que se va a registrar.</param>
    /// <returns>La respuesta de autenticación o el error de dominio convertido a HTTP.</returns>
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _service.RegisterAsync(dto);

        return result.IsSuccess
            ? Ok(result.Value)
            : ToHttpError(result.Error);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AutentificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    /// <summary>Valida las credenciales e inicia sesión.</summary>
    /// <param name="dto">Correo y contraseña del usuario.</param>
    /// <returns>La respuesta de autenticación o un error de dominio convertido a HTTP.</returns>
    public async Task<IActionResult> Login([FromBody] LogginDto dto)
    {
        var result = await _service.LoginAsync(dto);

        return result.IsSuccess
            ? Ok(result.Value)
            : ToHttpError(result.Error);
    }
}