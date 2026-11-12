using CSharpFunctionalExtensions;
using FuncoApi.Domain;
using FuncoApi.Dto.UserDto;
using FuncoApi.Email.service;
using FuncoApi.Mappers;
using FuncoApi.Repositories.userRepository;
using FuncoApi.Tokens;

namespace FuncoApi.Services.Authorification;

/// <summary>Implementa el registro y el inicio de sesión de usuarios.</summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _repository;

    private readonly IJwtTokens _jwtService;
    
    private readonly IEmailService _emailService;

    /// <summary>Inicializa el servicio con el repositorio de usuarios y el generador JWT.</summary>
    /// <param name="repository">Repositorio empleado para consultar y crear usuarios.</param>
    /// <param name="jwtService">Servicio que genera los tokens de autenticación.</param>
    /// <param name="emailService">Servicio para enviar correos electrónicos.</param>
    public AuthService(IUserRepository repository, IJwtTokens jwtService, IEmailService emailService)
    {
        _repository = repository;
        _jwtService = jwtService;
        _emailService = emailService;
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
        
        await _emailService.SendEmailAsync(
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
        await _emailService.SendEmailAsync(
            usuario.Email,
            "Bienvenido de nuevo",
            $"Hola {usuario.Nombre}, tu cuenta se ha iniciado sesión correctamente.");
        return Result.Success<AutentificationResponseDto, DomainError>(response);
    }
}