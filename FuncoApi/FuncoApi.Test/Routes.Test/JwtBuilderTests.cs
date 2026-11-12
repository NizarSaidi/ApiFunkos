using FunkoApi.Routes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;

namespace FuncoApi.Test.Routes.Test;

/// <summary>Comprueba la configuración de autenticación JWT en el contenedor de servicios.</summary>
public class JwtBuilderTests
{
    /// <summary>Comprueba que JWTBuilder se inicializa y registra autenticación con opciones válidas.</summary>
    [Fact]
    public void ConfigureJWT_RegistersAuthentication_WhenSettingsArePresent()
    {
        var builder = CreateBuilder(withSettings: true);

        var jwtBuilder = new JWTBuilder(builder);
        jwtBuilder.ConfigureJWT();

        Assert.Contains(builder.Services, descriptor =>
            descriptor.ServiceType == typeof(IAuthenticationSchemeProvider));
    }

    /// <summary>Comprueba que falta de configuración obligatoria produce un error explícito.</summary>
    [Fact]
    public void Constructor_Throws_WhenJwtSettingsAreMissing()
    {
        var builder = CreateBuilder(withSettings: false);

        var exception = Assert.Throws<InvalidOperationException>(() => new JWTBuilder(builder));

        Assert.Contains("clave JWT", exception.Message);
    }

    private static WebApplicationBuilder CreateBuilder(bool withSettings)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.Sources.Clear();
        if (withSettings)
        {
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-signing-key-with-at-least-32-bytes",
                ["Jwt:Issuer"] = "FuncoApi.Tests",
                ["Jwt:Audience"] = "FuncoApi.Tests.Client"
            });
        }

        return builder;
    }
}
