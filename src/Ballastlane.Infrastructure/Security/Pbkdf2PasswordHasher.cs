using System.Security.Cryptography;
using System.Text;
using Ballastlane.Application.Ports;

namespace Ballastlane.Infrastructure.Security;

/// <summary>
/// PBKDF2/SHA-256 implementation of IPasswordHasher.
///
/// Pattern: Strategy — swappable via IPasswordHasher without changing AuthService.
/// Security:
///   - 256-bit random salt per user (via GenerateSalt).
///   - 100 000 PBKDF2 iterations with SHA-256 (NIST SP 800-132 compliant).
///   - CryptographicOperations.FixedTimeEquals prevents timing attacks in Verify().
/// SOLID: SRP — only responsible for password hashing; DIP satisfied by implementing the port.
/// </summary>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int Iterations  = 100_000;
    private const int HashBytes   = 32;   // 256-bit output
    private const int SaltBytes   = 32;   // 256-bit salt

    public string GenerateSalt()
    {
        var saltBytes = RandomNumberGenerator.GetBytes(SaltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    public string Hash(string password, string salt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(salt);

        var saltBytes     = Convert.FromBase64String(salt);
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            passwordBytes,
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA256,
            HashBytes);

        return Convert.ToBase64String(hash);
    }

    public bool Verify(string password, string salt, string hash)
    {
        // Gracefully return false for empty input rather than throwing —
        // a missing password can never match a stored hash.
        if (string.IsNullOrEmpty(password)) return false;

        var expected = Hash(password, salt);

        // Constant-time comparison — prevents timing oracle attacks
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(expected),
            Convert.FromBase64String(hash));
    }
}
