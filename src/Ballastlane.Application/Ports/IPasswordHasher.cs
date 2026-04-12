namespace Ballastlane.Application.Ports;

/// <summary>
/// Port: Password hashing abstraction.
/// SOLID: DIP — Application depends on this interface; Infrastructure provides the implementation.
/// Pattern: Strategy — different hash algorithms (PBKDF2, Argon2) can be plugged in.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Produces a deterministic hash of <paramref name="password"/> using <paramref name="salt"/>.</summary>
    string Hash(string password, string salt);

    /// <summary>Generates a cryptographically random Base64-encoded salt.</summary>
    string GenerateSalt();

    /// <summary>Constant-time comparison to prevent timing attacks.</summary>
    bool Verify(string password, string salt, string hash);
}
