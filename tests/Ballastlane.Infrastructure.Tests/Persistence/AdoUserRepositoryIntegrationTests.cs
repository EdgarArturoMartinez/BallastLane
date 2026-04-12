using Ballastlane.Application.Ports;
using Ballastlane.Domain.Entities;
using Ballastlane.Domain.ValueObjects;
using Ballastlane.Infrastructure.Persistence.Repositories;

namespace Ballastlane.Infrastructure.Tests.Persistence;

/// <summary>
/// Integration tests for AdoUserRepository.
/// Requires a running SQL Server — see DatabaseFixture for connection details.
/// Run with:  SQLSERVER_INTEGRATION_CONNSTR="..." dotnet test --filter "Category=Integration"
/// </summary>
[Trait("Category", "Integration")]
[Collection("Database")]
public sealed class AdoUserRepositoryIntegrationTests
{
    private readonly IUserRepository _sut;

    public AdoUserRepositoryIntegrationTests(DatabaseFixture db)
    {
        _sut = new AdoUserRepository(db.Executor);
    }

    [Fact]
    public async Task CreateAsync_Then_GetByIdAsync_Returns_Same_User()
    {
        var user = BuildUser("alice");

        await _sut.CreateAsync(user);
        var found = await _sut.GetByIdAsync(user.Id);

        Assert.NotNull(found);
        Assert.Equal(user.Id,       found.Id);
        Assert.Equal(user.Username, found.Username);
        Assert.Equal(user.Email.Value, found.Email.Value);
        Assert.Equal(user.Role,     found.Role);
    }

    [Fact]
    public async Task GetByUsernameAsync_Returns_User_When_Exists()
    {
        var user = BuildUser("bob");
        await _sut.CreateAsync(user);

        var found = await _sut.GetByUsernameAsync(user.Username);

        Assert.NotNull(found);
        Assert.Equal(user.Id, found.Id);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Null_When_Not_Found()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUsernameAsync_Returns_Null_When_Not_Found()
    {
        var result = await _sut.GetByUsernameAsync("nonexistent_user");

        Assert.Null(result);
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static User BuildUser(string prefix) =>
        new(
            id:           Guid.NewGuid(),
            username:     $"{prefix}_{Guid.NewGuid():N}",
            email:        Email.Create($"{prefix}_{Guid.NewGuid():N}@test.com"),
            passwordHash: "hashed",
            salt:         "salt",
            role:         "User");
}
