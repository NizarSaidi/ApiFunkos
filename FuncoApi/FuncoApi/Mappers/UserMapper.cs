using FuncoApi.Dto.UserDto;
using FuncoApi.Entity;
using FuncoApi.Models;

namespace FuncoApi.Mappers;

public static class UserMapper
{
    /// <summary>Convierte los datos de creación en un modelo sin ID asignado.</summary>
    public static User ToModel(RegisterDto dto) => new()
    {
        Id = Guid.Empty,
        Nombre = dto.Name,
        Email = dto.Email,
        Password = dto.Password
    };

    /// <summary>Convierte el modelo de usuario y el token generado a la respuesta de autenticación.</summary>
    public static AutentificationResponseDto ToResponseDto(User usuario, string token) => new()
    {
        Token = token,
        Username = usuario.Nombre,
        Rol = usuario.Rol
    };

    /// <summary>Convierte una entidad persistida en el modelo de aplicación.</summary>
    public static User ToModel(UserEntity entity) => new()
    {
        Id = entity.Id,
        Nombre = entity.Nombre,
        Email = entity.Email,
        Password = entity.Password,
        Rol = entity.Rol
    };

    /// <summary>Convierte un modelo en la entidad persistible.</summary>
    public static UserEntity ToEntity(User usuario) => new()
    {
        Id = usuario.Id,
        Nombre = usuario.Nombre,
        Email = usuario.Email,
        Password = usuario.Password,
        Rol = usuario.Rol
    };
}