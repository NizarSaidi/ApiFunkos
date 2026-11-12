using FuncoApi.Domain;
using FuncoApi.Dto.UserDto;
using FuncoApi.Email.service;
using FuncoApi.Models;
using FuncoApi.Repositories.userRepository;
using FuncoApi.Services.Authorification;
using FuncoApi.Tokens;
using Moq;

namespace FuncoApi.Test.Services.Test;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _repository = new();
    private readonly Mock<IJwtTokens> _jwtTokens = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(_repository.Object, _jwtTokens.Object, _emailService.Object);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsConflictWhenNameExists()
    {
        _repository.Setup(repository => repository.ExistsByNameAsync("Ana"))
            .ReturnsAsync(true);

        var result = await _service.RegisterAsync(new RegisterDto
        {
            Name = "Ana",
            Email = "ana@example.com",
            Password = "secret"
        });

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrorCode.Conflict, result.Error.Code);
        _repository.Verify(repository => repository.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    /// <summary>Comprueba que el registro rechaza un correo ya utilizado.</summary>
    [Fact]
    public async Task RegisterAsync_ReturnsConflictWhenEmailExists()
    {
        _repository.Setup(repository => repository.ExistsByEmailAsync("ana@example.com"))
            .ReturnsAsync(true);

        var result = await _service.RegisterAsync(new RegisterDto
        {
            Name = "Ana",
            Email = "ana@example.com",
            Password = "secret"
        });

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrorCode.Conflict, result.Error.Code);
        _repository.Verify(repository => repository.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_CreatesUserAndReturnsAuthenticationResponse()
    {
        _repository.Setup(repository => repository.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);
        _jwtTokens.Setup(tokens => tokens.GenerateToken(It.IsAny<User>()))
            .Returns("jwt-token");

        var result = await _service.RegisterAsync(new RegisterDto
        {
            Name = "Ana",
            Email = "ana@example.com",
            Password = "secret"
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("Ana", result.Value.Username);
        Assert.Equal("User", result.Value.Rol);
        Assert.Equal("jwt-token", result.Value.Token);
        _repository.Verify(repository => repository.CreateAsync(It.Is<User>(user =>
            user.Email == "ana@example.com" &&
            user.Rol == "User" &&
            BCrypt.Net.BCrypt.Verify("secret", user.Password))), Times.Once);
    }

    /// <summary>Comprueba que un fallo al persistir el usuario devuelve un error de dominio.</summary>
    [Fact]
    public async Task RegisterAsync_ReturnsPersistenceFailure_WhenRepositoryCannotCreateUser()
    {
        _repository.Setup(repository => repository.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User?)null);

        var result = await _service.RegisterAsync(new RegisterDto
        {
            Name = "Ana",
            Email = "ana@example.com",
            Password = "secret"
        });

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrorCode.PersistenceFailure, result.Error.Code);
    }

    [Fact]
    public async Task LoginAsync_ReturnsUnauthorizedFailureForInvalidCredentials()
    {
        _repository.Setup(repository => repository.GetByEmailAsync("ana@example.com"))
            .ReturnsAsync((User?)null);

        var result = await _service.LoginAsync(new LogginDto
        {
            Email = "ana@example.com",
            Password = "incorrect"
        });

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrorCode.Unauthorized, result.Error.Code);
    }

    /// <summary>Comprueba que el inicio de sesión rechaza una contraseña incorrecta.</summary>
    [Fact]
    public async Task LoginAsync_ReturnsUnauthorized_WhenPasswordIsIncorrect()
    {
        _repository.Setup(repository => repository.GetByEmailAsync("ana@example.com"))
            .ReturnsAsync(new User
            {
                Email = "ana@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("correct"),
                Nombre = "Ana",
                Rol = "User"
            });

        var result = await _service.LoginAsync(new LogginDto
        {
            Email = "ana@example.com",
            Password = "incorrect"
        });

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrorCode.Unauthorized, result.Error.Code);
    }
}
