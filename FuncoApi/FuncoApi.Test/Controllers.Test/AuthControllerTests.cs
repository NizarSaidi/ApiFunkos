using CSharpFunctionalExtensions;
using FuncoApi.Controlers;
using FuncoApi.Domain;
using FuncoApi.Dto.UserDto;
using FuncoApi.Services.Authorification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FuncoApi.Test.Controllers.Test;

/// <summary>Comprueba la traducción de resultados de autenticación a respuestas HTTP.</summary>
public class AuthControllerTests
{
    private readonly Mock<IAuthService> _service = new();

    /// <summary>Comprueba que el registro devuelve el DTO de éxito sin envolverlo en Result.</summary>
    [Fact]
    public async Task Register_ReturnsResponseDto_WhenSuccessful()
    {
        var response = new AutentificationResponseDto
        {
            Token = "token",
            Username = "Ana",
            Rol = "User"
        };
        _service.Setup(service => service.RegisterAsync(It.IsAny<RegisterDto>()))
            .ReturnsAsync(Result.Success<AutentificationResponseDto, DomainError>(response));

        var result = await new AuthController(_service.Object).Register(new RegisterDto());

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, ok.Value);
    }

    /// <summary>Comprueba que el registro convierte un conflicto de dominio en HTTP 409.</summary>
    [Fact]
    public async Task Register_ReturnsConflict_WhenUserAlreadyExists()
    {
        var error = UserErrors.DuplicateEmail("ana@example.com");
        _service.Setup(service => service.RegisterAsync(It.IsAny<RegisterDto>()))
            .ReturnsAsync(Result.Failure<AutentificationResponseDto, DomainError>(error));

        var result = await new AuthController(_service.Object).Register(new RegisterDto());

        var conflict = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, conflict.StatusCode);
        Assert.Same(error, conflict.Value);
    }

    /// <summary>Comprueba que las credenciales inválidas se devuelven como HTTP 401.</summary>
    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
    {
        var error = UserErrors.InvalidCredentials();
        _service.Setup(service => service.LoginAsync(It.IsAny<LogginDto>()))
            .ReturnsAsync(Result.Failure<AutentificationResponseDto, DomainError>(error));

        var result = await new AuthController(_service.Object).Login(new LogginDto());

        var unauthorized = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorized.StatusCode);
        Assert.Same(error, unauthorized.Value);
    }

    /// <summary>Comprueba que un error de persistencia se convierte en HTTP 500.</summary>
    [Fact]
    public async Task Register_ReturnsInternalServerError_WhenPersistenceFails()
    {
        var error = UserErrors.PersistenceFailure();
        _service.Setup(service => service.RegisterAsync(It.IsAny<RegisterDto>()))
            .ReturnsAsync(Result.Failure<AutentificationResponseDto, DomainError>(error));

        var result = await new AuthController(_service.Object).Register(new RegisterDto());

        var failure = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, failure.StatusCode);
        Assert.Same(error, failure.Value);
    }
}
