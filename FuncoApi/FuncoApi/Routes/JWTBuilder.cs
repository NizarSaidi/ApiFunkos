using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace FunkoApi.Routes;

/// <summary>Configura autenticación y autorización JWT para la aplicación ASP.NET Core.</summary>
public class JWTBuilder
{
    private readonly WebApplicationBuilder _builder;
    private readonly string _jwtKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;

    /// <summary>Inicializa la configuración JWT desde la configuración de la aplicación.</summary>
    /// <param name="builder">Constructor de aplicación al que se añadirán los servicios.</param>
    public JWTBuilder(WebApplicationBuilder builder)
    {
        _builder = builder;
        _jwtKey = builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("La clave JWT no está configurada.");
        _jwtIssuer = builder.Configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("El emisor JWT no está configurado.");
        _jwtAudience = builder.Configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("La audiencia JWT no está configurada.");
    }

    /// <summary>Registra la validación de firma, emisor, audiencia y vigencia de los JWT.</summary>
    public void ConfigureJWT()
    {
        _builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtAudience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey)),
                    ValidateLifetime = true
                };
            });
        _builder.Services.AddAuthorization();
    }
}