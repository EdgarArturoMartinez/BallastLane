using Ballastlane.Application.DTOs;
using Ballastlane.Application.Ports;
using Ballastlane.Application.Services;
using Ballastlane.Domain.Entities;
using Ballastlane.Domain.Exceptions;
using Ballastlane.Domain.ValueObjects;

namespace Ballastlane.Application.Tests.Services;

/// <summary>
/// TDD tests for AuthService.
/// All dependencies mocked — no real hashing or JWT signing.
/// Security rules validated (duplicate username, short password, wrong credentials).
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtMock = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_userRepoMock.Object, _hasherMock.Object, _jwtMock.Object);
    }

    // ── RegisterAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_ValidRequest_CreatesUserAndReturnsDto()
    {
        _userRepoMock.Setup(r => r.GetByUsernameAsync("alice", default))
                     .ReturnsAsync((User?)null);
        _hasherMock.Setup(h => h.GenerateSalt()).Returns("salt123");
        _hasherMock.Setup(h => h.Hash("P@ssword1", "salt123")).Returns("hashed");
        _userRepoMock.Setup(r => r.CreateAsync(It.IsAny<User>(), default))
                     .Returns(Task.CompletedTask);

        var request = new RegisterRequest("alice", "alice@example.com", "P@ssword1");
        var dto = await _sut.RegisterAsync(request);

        Assert.Equal("alice", dto.Username);
        Assert.Equal("alice@example.com", dto.Email);
        Assert.Equal("User", dto.Role);
        _userRepoMock.Verify(r => r.CreateAsync(It.IsAny<User>(), default), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ThrowsDomainException()
    {
        var existing = new User(
            Guid.NewGuid(), "alice",
            Email.Create("alice@example.com"), "hash", "salt");

        _userRepoMock.Setup(r => r.GetByUsernameAsync("alice", default))
                     .ReturnsAsync(existing);

        var request = new RegisterRequest("alice", "other@example.com", "P@ssword1");

        await Assert.ThrowsAsync<DomainException>(() => _sut.RegisterAsync(request));
    }

    [Theory]
    [InlineData("short")]    // < 8 chars
    [InlineData("")]
    [InlineData("       ")]
    public async Task RegisterAsync_WeakPassword_ThrowsDomainException(string password)
    {
        _userRepoMock.Setup(r => r.GetByUsernameAsync(It.IsAny<string>(), default))
                     .ReturnsAsync((User?)null);

        var request = new RegisterRequest("bob", "bob@example.com", password);

        await Assert.ThrowsAsync<DomainException>(() => _sut.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_NullRequest_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.RegisterAsync(null!));
    }

    // ── LoginAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokenResponse()
    {
        var user = new User(
            Guid.NewGuid(), "alice",
            Email.Create("alice@example.com"), "hashed", "salt123");

        _userRepoMock.Setup(r => r.GetByUsernameAsync("alice", default))
                     .ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("P@ssword1", "salt123", "hashed"))
                   .Returns(true);
        _jwtMock.Setup(j => j.GenerateToken(user)).Returns("jwt.token.here");

        var request = new LoginRequest("alice", "P@ssword1");
        var response = await _sut.LoginAsync(request);

        Assert.Equal("jwt.token.here", response.Token);
        Assert.Equal("alice", response.Username);
        Assert.Equal("User", response.Role);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsDomainException()
    {
        _userRepoMock.Setup(r => r.GetByUsernameAsync(It.IsAny<string>(), default))
                     .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.LoginAsync(new LoginRequest("ghost", "P@ssword1")));
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsDomainException()
    {
        var user = new User(
            Guid.NewGuid(), "alice",
            Email.Create("alice@example.com"), "hashed", "salt");

        _userRepoMock.Setup(r => r.GetByUsernameAsync("alice", default))
                     .ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                   .Returns(false);

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.LoginAsync(new LoginRequest("alice", "WrongPass")));
    }

    [Fact]
    public async Task LoginAsync_NullRequest_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.LoginAsync(null!));
    }
}
