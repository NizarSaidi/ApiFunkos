using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FuncoApi.Models;
using Microsoft.IdentityModel.Tokens;
    
namespace FuncoApi.Tokens;


/// <summary>
/// Servicio para generacion y validacion de JWT tokens.
/// </summary>
public class JwtTokensService(IConfiguration configuration)
{
    private readonly string _secretKey = configuration["Jwt:Secret"]
        ?? throw new InvalidOperationException("JWT Secret no configurado");
    private readonly string _issuer = configuration["Jwt:Issuer"] ?? "ProductosApi";
    private readonly string _audience = configuration["Jwt:Audience"] ?? "ProductosApiClients";
    private readonly int _accessTokenExpiryMinutes = configuration.GetValue<int>("Jwt:ExpirationMinutes", 15);
    private readonly int _refreshTokenExpiryDays = configuration.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7);

    /// <summary>
    /// Segundos de validez del access token (para ExpiresIn de AuthResponse).
    /// </summary>
    public int AccessTokenExpirySeconds => _accessTokenExpiryMinutes * 60;

    /// <summary>
    /// Genera un JWT access token para el usuario dado.
    /// </summary>
    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new("username", user.Nombre),
            new("role", user.Rol.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Genera un refresh token aleatorio de 32 bytes.
    /// </summary>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber)
            .Replace("/", "_")
            .Replace("+", "-");
    }

    /// <summary>
    /// Valida un JWT token y devuelve el ClaimsPrincipal o null.
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }
}