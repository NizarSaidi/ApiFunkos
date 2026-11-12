using FuncoApi.Dto.UserDto;
using FuncoApi.Entity;
using FuncoApi.Mappers;
using FuncoApi.Models;

namespace FuncoApi.Test.Mappers.Test;

public class UserMapperTests
{
    [Fact]
    public void ToModel_MapsRegistrationDto()
    {
        var dto = new RegisterDto
        {
            Name = "Ana",
            Email = "ana@example.com",
            Password = "secret"
        };

        var model = UserMapper.ToModel(dto);

        Assert.Equal(Guid.Empty, model.Id);
        Assert.Equal(dto.Name, model.Nombre);
        Assert.Equal(dto.Email, model.Email);
        Assert.Equal(dto.Password, model.Password);
    }

    [Fact]
    public void ToModel_MapsLoginDtoCredentials()
    {
        var dto = new LogginDto
        {
            Email = "ana@example.com",
            Password = "secret"
        };

        var model = UserMapper.ToModel(dto);

        Assert.Equal(dto.Email, model.Email);
        Assert.Equal(dto.Password, model.Password);
        Assert.Equal(Guid.Empty, model.Id);
        Assert.Equal(string.Empty, model.Nombre);
    }

    [Fact]
    public void ToResponseDto_MapsPublicAuthenticationFields()
    {
        var model = new User
        {
            Nombre = "Ana",
            Rol = "User",
            Email = "ana@example.com",
            Password = "secret"
        };

        var dto = UserMapper.ToResponseDto(model, "jwt-token");

        Assert.Equal("jwt-token", dto.Token);
        Assert.Equal(model.Nombre, dto.Username);
        Assert.Equal(model.Rol, dto.Rol);
    }

    [Fact]
    public void ToModel_MapsPersistedUserFields()
    {
        var entity = new UserEntity
        {
            Id = Guid.NewGuid(),
            Nombre = "Ana",
            Email = "ana@example.com",
            Password = "hashed-password",
            Rol = "Admin"
        };

        var model = UserMapper.ToModel(entity);

        Assert.Equal(entity.Id, model.Id);
        Assert.Equal(entity.Nombre, model.Nombre);
        Assert.Equal(entity.Email, model.Email);
        Assert.Equal(entity.Password, model.Password);
        Assert.Equal(entity.Rol, model.Rol);
    }

    [Fact]
    public void ToEntity_MapsUserFieldsForPersistence()
    {
        var model = new User
        {
            Id = Guid.NewGuid(),
            Nombre = "Ana",
            Email = "ana@example.com",
            Password = "hashed-password",
            Rol = "Admin"
        };

        var entity = UserMapper.ToEntity(model);

        Assert.Equal(model.Id, entity.Id);
        Assert.Equal(model.Nombre, entity.Nombre);
        Assert.Equal(model.Email, entity.Email);
        Assert.Equal(model.Password, entity.Password);
        Assert.Equal(model.Rol, entity.Rol);
    }
}
