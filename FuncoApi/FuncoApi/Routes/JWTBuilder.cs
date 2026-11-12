using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace FunkoApi.Routes;

public class JWTBuilder
{
    private static WebApplicationBuilder _builder;

    public JWTBuilder(WebApplicationBuilder builder)
    {
        _builder = builder;
    }

    private readonly string _jwtKey =
        _builder.Configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("La clave JWT no está configurada.");

    private readonly string _jwtIssuer =
        _builder.Configuration["Jwt:Issuer"]
        ?? throw new InvalidOperationException("El emisor JWT no está configurado.");

    private readonly string _jwtAudience =
        _builder.Configuration["Jwt:Audience"]
        ?? throw new InvalidOperationException("La audiencia JWT no está configurada.");

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