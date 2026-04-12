using Ballastlane.Domain.Entities;
using Ballastlane.Domain.Exceptions;
using Ballastlane.Domain.ValueObjects;

namespace Ballastlane.Domain.Tests.Entities;

public class UserTests
{
    private readonly Guid _id = Guid.NewGuid();
    private readonly Email _email = Email.Create("john@example.com");

    // ── Construction ──────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ValidArgs_CreatesUser()
    {
        var user = new User(_id, "john", _email, "hash123", "salt123");

        Assert.Equal(_id, user.Id);
        Assert.Equal("john", user.Username);
        Assert.Equal("john@example.com", user.Email.Value);
        Assert.Equal("hash123", user.PasswordHash);
        Assert.Equal("salt123", user.Salt);
        Assert.Equal("User", user.Role);
    }

    [Fact]
    public void Constructor_EmptyId_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new User(Guid.Empty, "john", _email, "hash", "salt"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_EmptyUsername_ThrowsDomainException(string? username)
    {
        Assert.Throws<DomainException>(() =>
            new User(_id, username!, _email, "hash", "salt"));
    }

    [Fact]
    public void Constructor_NullEmail_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new User(_id, "john", null!, "hash", "salt"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyPasswordHash_ThrowsDomainException(string badHash)
    {
        Assert.Throws<DomainException>(() =>
            new User(_id, "john", _email, badHash, "salt"));
    }

    // ── UpdateProfile ─────────────────────────────────────────────────────────

    [Fact]
    public void UpdateProfile_ValidArgs_UpdatesUsernameAndEmail()
    {
        var user = new User(_id, "john", _email, "hash", "salt");
        var newEmail = Email.Create("jane@example.com");

        user.UpdateProfile("jane", newEmail);

        Assert.Equal("jane", user.Username);
        Assert.Equal("jane@example.com", user.Email.Value);
    }

    [Fact]
    public void UpdateProfile_EmptyUsername_ThrowsDomainException()
    {
        var user = new User(_id, "john", _email, "hash", "salt");

        Assert.Throws<DomainException>(() => user.UpdateProfile("", _email));
    }
}
