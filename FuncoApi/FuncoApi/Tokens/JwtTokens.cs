using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FuncoApi.Models;
using Microsoft.IdentityModel.Tokens;
using FunkoApi.Routes;
using Microsoft.Extensions.Options;

namespace FuncoApi.Tokens;


/// <summary>
/// Servicio para generación y validación de tokens JWT.
/// </summary>
public class JwtService : IJwtTokens
{
    private readonly JwtSettings _settings;

    /// <summary>Inicializa el servicio con las opciones de configuración JWT.</summary>
    /// <param name="settings">Emisor, audiencia, clave de firma y duración de los tokens.</param>
    public JwtService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    /// <inheritdoc />
    public string GenerateToken(User usuario)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier,
                usuario.Id.ToString()),

            new(ClaimTypes.Name,
                usuario.Nombre),

            new(ClaimTypes.Role,
                usuario.Rol)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _settings.Key));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow
                    .AddMinutes(
                        _settings.DurationInMinutes),
                signingCredentials:
                credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    /// <inheritdoc />
    public User? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _settings.Issuer,
                ValidateAudience = true,
                ValidAudience = _settings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)),
                ValidateLifetime = true
            }, out _);

            var idValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(idValue, out var id))
            {
                return null;
            }

            return new User
            {
                Id = id,
                Nombre = principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                Rol = principal.FindFirstValue(ClaimTypes.Role) ?? string.Empty
            };
        }
        catch (SecurityTokenException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}