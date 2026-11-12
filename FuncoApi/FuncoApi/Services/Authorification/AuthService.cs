using CSharpFunctionalExtensions;
using FuncoApi.Domain;
using FuncoApi.Dto.UserDto;
using FuncoApi.Email.service;
using FuncoApi.Mappers;
using FuncoApi.Repositories.userRepository;
using FuncoApi.Tokens;
using MailKit;
using Microsoft.Extensions.Logging;

namespace FuncoApi.Services.Authorification;

/// <summary>Implementa el registro y el inicio de sesión de usuarios.</summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _repository;

    private readonly IJwtTokens _jwtService;
    
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    /// <summary>Inicializa el servicio con el repositorio de usuarios y el generador JWT.</summary>
    /// <param name="repository">Repositorio empleado para consultar y crear usuarios.</param>
    /// <param name="jwtService">Servicio que genera los tokens de autenticación.</param>
    /// <param name="emailService">Servicio para enviar correos electrónicos.</param>
    /// <param name="logger">Registrador para informar fallos no críticos al enviar notificaciones.</param>
    public AuthService(
        IUserRepository repository,
        IJwtTokens jwtService,
        IEmailService emailService,
        ILogger<AuthService> logger)
    {
        _repository = repository;
        _jwtService = jwtService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<AutentificationResponseDto, DomainError>> RegisterAsync(RegisterDto dto)
    {
        if (await _repository.ExistsByNameAsync(dto.Name))
        {
            return Result.Failure<AutentificationResponseDto, DomainError>(
                UserErrors.DuplicateName(dto.Name));
        }

        if (await _repository.ExistsByEmailAsync(dto.Email))
        {
            return Result.Failure<AutentificationResponseDto, DomainError>(
                UserErrors.DuplicateEmail(dto.Email));
        }

        var usuario = UserMapper.ToModel(dto);
        usuario.Id = Guid.NewGuid();
        usuario.Password = BCrypt.Net.BCrypt.HashPassword(usuario.Password);
        usuario.Rol = "User";

        var created = await _repository.CreateAsync(usuario);
        if (created is null)
        {
            return Result.Failure<AutentificationResponseDto, DomainError>(
                UserErrors.PersistenceFailure());
        }

        var response = UserMapper.ToResponseDto(created, _jwtService.GenerateToken(created));
        
        await SendNotificationEmailAsync(
            usuario.Email,
            "Bienvenido",
            $"Hola {usuario.Nombre}, tu cuenta ha sido creada correctamente.");
        
        return Result.Success<AutentificationResponseDto, DomainError>(response);
    }

    /// <inheritdoc />
    public async Task<Result<AutentificationResponseDto, DomainError>> LoginAsync(LogginDto dto)
    {
        var usuario = await _repository.GetByEmailAsync(dto.Email);

        if (usuario == null)
        {
            return Result.Failure<AutentificationResponseDto, DomainError>(
                UserErrors.InvalidCredentials());
        }

        var valid = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.Password);

        if (!valid)
        {
            return Result.Failure<AutentificationResponseDto, DomainError>(
                UserErrors.InvalidCredentials());
        }

        var response = UserMapper.ToResponseDto(usuario, _jwtService.GenerateToken(usuario));
        await SendNotificationEmailAsync(
            usuario.Email,
            "Bienvenido de nuevo",
            $"Hola {usuario.Nombre}, tu cuenta se ha iniciado sesión correctamente.");
        return Result.Success<AutentificationResponseDto, DomainError>(response);
    }

    private async Task SendNotificationEmailAsync(string to, string subject, string body)
    {
        try
        {
            await _emailService.SendEmailAsync(to, subject, body);
        }
        catch (Exception exception) when (
            exception is ProtocolException or
            CommandException or
            MailKit.Security.AuthenticationException or
            IOException or
            System.Net.Sockets.SocketException)
        {
            _logger.LogWarning(exception, "No se pudo enviar el correo de autenticación.");
        }
    }
}