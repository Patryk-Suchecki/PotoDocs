using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PotoDocs.API.Entities;
using PotoDocs.API.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PotoDocs.API.Services;

public interface ITokenService
{
    string GenerateJWT(User user, IEnumerable<Claim>? additionalClaims = null);
}

public sealed class TokenService : ITokenService
{
    private readonly AuthenticationSettings _settings;
    private readonly SymmetricSecurityKey _key;

    public TokenService(IOptions<AuthenticationSettings> options)
    {
        _settings = options.Value;

        if (string.IsNullOrWhiteSpace(_settings.JwtKey))
            throw new ArgumentNullException(nameof(_settings.JwtKey), "JWT Key nie jest skonfigurowany.");

        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.JwtKey));
    }

    public static TokenValidationParameters GetTokenValidationParameters(IConfiguration configuration)
    {
        var jwtKey = configuration["Authentication:JwtKey"]
                     ?? throw new InvalidOperationException("JwtKey is missing in config");
        var jwtIssuer = configuration["Authentication:JwtIssuer"];

        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    }

    public string GenerateJWT(User user, IEnumerable<Claim>? additionalClaims = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Role, user.Role.Name),
            new(ClaimTypes.Email, user.Email)
        };

        if (additionalClaims != null)
        {
            claims.AddRange(additionalClaims);
        }

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.JwtIssuer,
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.JwtExpireInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}