using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ballastlane.Domain.Entities;
using Ballastlane.Domain.ValueObjects;
using Ballastlane.Infrastructure.Security;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Ballastlane.Infrastructure.Tests.Security;

/// <summary>
/// Unit tests for JwtTokenGenerator.
///
/// TDD: specify that the generator produces valid, parseable HS256 tokens
/// with the correct claims (sub, unique_name, role, jti) and expiry.
/// </summary>
public sealed class JwtTokenGeneratorTests
{
    private static readonly JwtSettings DefaultSettings = new()
    {
        Secret        = "super-secret-key-for-unit-tests-32ch!!",
        Issuer        = "ballastlane-test",
        Audience      = "ballastlane-test-client",
        ExpiryMinutes = 60,
    };

    private static User MakeUser(string role = "User") =>
        new(
            Guid.NewGuid(),
            "testuser",
            Email.Create("test@example.com"),
            "hashedPassword",
            "saltValue",
            role);

    // ── Token generation ────────────────────────────────────────────────

    [Fact]
    public void GenerateToken_Returns_NonEmpty_String()
    {
        var sut   = new JwtTokenGenerator(DefaultSettings);
        var user  = MakeUser();
        var token = sut.GenerateToken(user);

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void GenerateToken_Returns_Well_Formed_Jwt()
    {
        var sut   = new JwtTokenGenerator(DefaultSettings);
        var user  = MakeUser();
        var token = sut.GenerateToken(user);

        // A JWT has exactly 3 dot-separated parts
        Assert.Equal(3, token.Split('.').Length);
    }

    // ── Claims ──────────────────────────────────────────────────────────

    [Fact]
    public void GenerateToken_Contains_Sub_Claim_Equal_To_UserId()
    {
        var sut    = new JwtTokenGenerator(DefaultSettings);
        var user   = MakeUser();
        var claims = ParseClaims(sut.GenerateToken(user));

        var sub = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        Assert.NotNull(sub);
        Assert.Equal(user.Id.ToString(), sub.Value);
    }

    [Fact]
    public void GenerateToken_Contains_UniqueName_Claim_Equal_To_Username()
    {
        var sut    = new JwtTokenGenerator(DefaultSettings);
        var user   = MakeUser();
        var claims = ParseClaims(sut.GenerateToken(user));

        var uniqueName = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName);
        Assert.NotNull(uniqueName);
        Assert.Equal(user.Username, uniqueName.Value);
    }

    [Fact]
    public void GenerateToken_Contains_Role_Claim()
    {
        var sut    = new JwtTokenGenerator(DefaultSettings);
        var user   = MakeUser(role: "Admin");
        var claims = ParseClaims(sut.GenerateToken(user));

        var role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        Assert.NotNull(role);
        Assert.Equal("Admin", role.Value);
    }

    [Fact]
    public void GenerateToken_Contains_Non_Empty_Jti_Claim()
    {
        var sut    = new JwtTokenGenerator(DefaultSettings);
        var user   = MakeUser();
        var claims = ParseClaims(sut.GenerateToken(user));

        var jti = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
        Assert.NotNull(jti);
        Assert.True(Guid.TryParse(jti.Value, out _), "jti must be a valid GUID");
    }

    // ── Expiry ──────────────────────────────────────────────────────────

    [Fact]
    public void GenerateToken_Expires_According_To_Settings()
    {
        var settings = new JwtSettings
        {
            Secret        = DefaultSettings.Secret,
            Issuer        = DefaultSettings.Issuer,
            Audience      = DefaultSettings.Audience,
            ExpiryMinutes = 30,
        };
        var sut      = new JwtTokenGenerator(settings);
        var user     = MakeUser();

        var handler    = new JwtSecurityTokenHandler();
        var jwtToken   = handler.ReadJwtToken(sut.GenerateToken(user));

        var expectedExpiry = DateTime.UtcNow.AddMinutes(30);
        // Allow ± 5 seconds to account for test execution time
        Assert.InRange(jwtToken.ValidTo, expectedExpiry.AddSeconds(-5), expectedExpiry.AddSeconds(5));
    }

    // ── Issuer / Audience ────────────────────────────────────────────────

    [Fact]
    public void GenerateToken_Sets_Correct_Issuer_And_Audience()
    {
        var sut     = new JwtTokenGenerator(DefaultSettings);
        var user    = MakeUser();
        var handler = new JwtSecurityTokenHandler();
        var jwt     = handler.ReadJwtToken(sut.GenerateToken(user));

        Assert.Equal(DefaultSettings.Issuer,   jwt.Issuer);
        Assert.Contains(DefaultSettings.Audience, jwt.Audiences);
    }

    // ── Constructor guard ────────────────────────────────────────────────

    [Fact]
    public void Constructor_Throws_When_Settings_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new JwtTokenGenerator(null!));
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static IEnumerable<Claim> ParseClaims(string token)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(DefaultSettings.Secret));

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidIssuer              = DefaultSettings.Issuer,
            ValidateAudience         = true,
            ValidAudience            = DefaultSettings.Audience,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = key,
            ClockSkew                = TimeSpan.Zero,
        };

        // Clear the default inbound claim-type mapping so JWT standard claim names
        // (e.g. "sub", "unique_name") are preserved as-is and not remapped to
        // long .NET URN types (ClaimTypes.NameIdentifier, etc.).
        var handler = new JwtSecurityTokenHandler();
        handler.InboundClaimTypeMap.Clear();

        var principal = handler.ValidateToken(token, validationParams, out _);
        return principal.Claims;
    }
}
