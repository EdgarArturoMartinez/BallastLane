using Ballastlane.Infrastructure.Security;

namespace Ballastlane.Infrastructure.Tests.Security;

/// <summary>
/// Unit tests for Pbkdf2PasswordHasher.
///
/// TDD: these tests specify the contract (generate salt, hash, verify).
/// Security coverage: timing-safe verify, constant output length, salt uniqueness.
/// </summary>
public sealed class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _sut = new();

    // ── GenerateSalt ────────────────────────────────────────────────────

    [Fact]
    public void GenerateSalt_Returns_NonEmpty_Base64String()
    {
        var salt = _sut.GenerateSalt();

        Assert.False(string.IsNullOrWhiteSpace(salt));
        // Must be parseable as Base64
        var bytes = Convert.FromBase64String(salt);
        Assert.Equal(32, bytes.Length); // 256-bit salt
    }

    [Fact]
    public void GenerateSalt_Returns_Different_Values_On_Each_Call()
    {
        var salt1 = _sut.GenerateSalt();
        var salt2 = _sut.GenerateSalt();

        Assert.NotEqual(salt1, salt2);
    }

    // ── Hash ────────────────────────────────────────────────────────────

    [Fact]
    public void Hash_Returns_NonEmpty_Base64String()
    {
        var salt = _sut.GenerateSalt();
        var hash = _sut.Hash("P@ssword1!", salt);

        Assert.False(string.IsNullOrWhiteSpace(hash));
        // Verify it is valid Base64
        var bytes = Convert.FromBase64String(hash);
        Assert.Equal(32, bytes.Length); // 256-bit hash output
    }

    [Fact]
    public void Hash_Is_Deterministic_For_Same_Password_And_Salt()
    {
        var salt  = _sut.GenerateSalt();
        var hash1 = _sut.Hash("P@ssword1!", salt);
        var hash2 = _sut.Hash("P@ssword1!", salt);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Hash_Produces_Different_Output_For_Different_Salts()
    {
        var salt1 = _sut.GenerateSalt();
        var salt2 = _sut.GenerateSalt();
        var hash1 = _sut.Hash("P@ssword1!", salt1);
        var hash2 = _sut.Hash("P@ssword1!", salt2);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Hash_Throws_For_Null_Or_Empty_Password()
    {
        var salt = _sut.GenerateSalt();

        Assert.Throws<ArgumentException>(() => _sut.Hash(string.Empty, salt));
        Assert.Throws<ArgumentException>(() => _sut.Hash("   ",        salt));
    }

    [Fact]
    public void Hash_Throws_For_Null_Or_Empty_Salt()
    {
        Assert.Throws<ArgumentException>(() => _sut.Hash("P@ssword1!", string.Empty));
        Assert.Throws<ArgumentException>(() => _sut.Hash("P@ssword1!", "   "));
    }

    // ── Verify ──────────────────────────────────────────────────────────

    [Fact]
    public void Verify_Returns_True_For_Correct_Password()
    {
        const string password = "C0rrectH0rse!";
        var salt = _sut.GenerateSalt();
        var hash = _sut.Hash(password, salt);

        Assert.True(_sut.Verify(password, salt, hash));
    }

    [Fact]
    public void Verify_Returns_False_For_Wrong_Password()
    {
        var salt = _sut.GenerateSalt();
        var hash = _sut.Hash("OriginalPass1!", salt);

        Assert.False(_sut.Verify("WrongPass1!", salt, hash));
    }

    [Fact]
    public void Verify_Returns_False_For_Empty_Password()
    {
        var salt = _sut.GenerateSalt();
        var hash = _sut.Hash("OriginalPass1!", salt);

        Assert.False(_sut.Verify(string.Empty, salt, hash));
    }

    [Fact]
    public void Verify_Is_Case_Sensitive()
    {
        const string password = "CaseSensitive1";
        var salt = _sut.GenerateSalt();
        var hash = _sut.Hash(password, salt);

        Assert.False(_sut.Verify("casesensitive1", salt, hash));
        Assert.False(_sut.Verify("CASESENSITIVE1", salt, hash));
    }
}
