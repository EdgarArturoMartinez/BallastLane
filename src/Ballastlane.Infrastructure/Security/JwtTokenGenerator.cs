using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ballastlane.Application.Ports;
using Ballastlane.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Ballastlane.Infrastructure.Security;

/// <summary>
/// HS256 JWT generator implementing IJwtTokenGenerator.
///
/// Pattern: Strategy — swap to RS256/ES256 without changing AuthService.
/// Security:
///   - Secret read from configuration (never hard-coded).
///   - Short expiry (60 min) — refresh token pattern can extend later.
///   - Claims: sub (userId), unique_name (username), role.
/// SOLID: SRP — only generates tokens; DIP satisfied by implementing the port.
/// </summary>
public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _settings;

    public JwtTokenGenerator(JwtSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,        user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Role,                    user.Role),
            new Claim(JwtRegisteredClaimNames.Jti,        Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer:             _settings.Issuer,
            audience:           _settings.Audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

/// <summary>Strongly-typed settings bound from appsettings.json → Jwt section.</summary>
public sealed class JwtSettings
{
    public string Secret        { get; init; } = string.Empty;
    public string Issuer        { get; init; } = string.Empty;
    public string Audience      { get; init; } = string.Empty;
    public int    ExpiryMinutes { get; init; } = 60;
}
