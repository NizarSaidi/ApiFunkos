using FuncoApi.Models;
using FuncoApi.Tokens;
using FunkoApi.Routes;
using Microsoft.Extensions.Options;

namespace FuncoApi.Test.Tokens.Test;

/// <summary>Comprueba la creación y validación de tokens JWT.</summary>
public class JwtServiceTests
{
    private static readonly JwtSettings Settings = new()
    {
        Key = "test-signing-key-with-at-least-32-bytes",
        Issuer = "FuncoApi.Tests",
        Audience = "FuncoApi.Tests.Client",
        DurationInMinutes = 30
    };

    /// <summary>Comprueba que un token válido recupera la identidad y el rol del usuario.</summary>
    [Fact]
    public void ValidateToken_ReturnsUser_WhenTokenIsValid()
    {
        var service = CreateService();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Nombre = "Ana",
            Rol = "Admin"
        };

        var token = service.GenerateToken(user);
        var validated = service.ValidateToken(token);

        Assert.NotNull(validated);
        Assert.Equal(user.Id, validated.Id);
        Assert.Equal(user.Nombre, validated.Nombre);
        Assert.Equal(user.Rol, validated.Rol);
    }

    /// <summary>Comprueba que un token malformado no es aceptado.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("not-a-jwt")]
    public void ValidateToken_ReturnsNull_WhenTokenIsInvalid(string token)
    {
        var validated = CreateService().ValidateToken(token);

        Assert.Null(validated);
    }

    /// <summary>Comprueba que un token firmado con otra clave no es aceptado.</summary>
    [Fact]
    public void ValidateToken_ReturnsNull_WhenSignatureIsInvalid()
    {
        var token = CreateService().GenerateToken(new User
        {
            Id = Guid.NewGuid(),
            Nombre = "Ana",
            Rol = "User"
        });
        var serviceWithDifferentKey = new JwtService(Options.Create(new JwtSettings
        {
            Key = "different-signing-key-with-32-bytes-long",
            Issuer = Settings.Issuer,
            Audience = Settings.Audience,
            DurationInMinutes = Settings.DurationInMinutes
        }));

        Assert.Null(serviceWithDifferentKey.ValidateToken(token));
    }

    private static JwtService CreateService() => new(Options.Create(Settings));
}
