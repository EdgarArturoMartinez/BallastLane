using Ballastlane.Application.Ports;
using Ballastlane.Domain.Entities;
using Ballastlane.Domain.ValueObjects;
using Ballastlane.Infrastructure.Persistence.Repositories;

namespace Ballastlane.Infrastructure.Tests.Persistence;

/// <summary>
/// Integration tests for AdoTaskRepository.
/// Requires a running SQL Server — see DatabaseFixture.
/// </summary>
[Trait("Category", "Integration")]
[Collection("Database")]
public sealed class AdoTaskRepositoryIntegrationTests
{
    private readonly IUserRepository _users;
    private readonly ITaskRepository _sut;

    public AdoTaskRepositoryIntegrationTests(DatabaseFixture db)
    {
        _users = new AdoUserRepository(db.Executor);
        _sut   = new AdoTaskRepository(db.Executor);
    }

    [Fact]
    public async Task CreateAsync_Then_GetByIdAsync_Returns_Same_Task()
    {
        var owner = await CreateOwnerAsync();
        var task  = new TaskItem(Guid.NewGuid(), "Buy milk", "Get 2 litres", owner.Id, null);

        await _sut.CreateAsync(task);
        var found = await _sut.GetByIdAsync(task.Id);

        Assert.NotNull(found);
        Assert.Equal(task.Id,          found.Id);
        Assert.Equal(task.Title,       found.Title);
        Assert.Equal(task.Description, found.Description);
        Assert.Equal(task.OwnerUserId, found.OwnerUserId);
        Assert.Equal(TaskItemStatus.Todo, found.Status);
    }

    [Fact]
    public async Task ListAsync_Returns_Created_Tasks()
    {
        var owner = await CreateOwnerAsync();
        var task  = new TaskItem(Guid.NewGuid(), $"Task_{Guid.NewGuid():N}", "Desc", owner.Id, null);

        await _sut.CreateAsync(task);
        var allTasks = await _sut.ListAsync();

        Assert.Contains(allTasks, t => t.Id == task.Id);
    }

    [Fact]
    public async Task UpdateAsync_Persists_Changes()
    {
        var owner = await CreateOwnerAsync();
        var task  = new TaskItem(Guid.NewGuid(), "Original", "Desc", owner.Id, null);
        await _sut.CreateAsync(task);

        task.Update("Updated title", "New desc", TaskItemStatus.InProgress, null);
        await _sut.UpdateAsync(task);

        var found = await _sut.GetByIdAsync(task.Id);
        Assert.NotNull(found);
        Assert.Equal("Updated title",         found.Title);
        Assert.Equal(TaskItemStatus.InProgress, found.Status);
    }

    [Fact]
    public async Task DeleteAsync_Removes_Task()
    {
        var owner = await CreateOwnerAsync();
        var task  = new TaskItem(Guid.NewGuid(), "To delete", "Desc", owner.Id, null);
        await _sut.CreateAsync(task);

        await _sut.DeleteAsync(task.Id);
        var found = await _sut.GetByIdAsync(task.Id);

        Assert.Null(found);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Null_When_Not_Found()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private async Task<User> CreateOwnerAsync()
    {
        var user = new User(
            id:           Guid.NewGuid(),
            username:     $"owner_{Guid.NewGuid():N}",
            email:        Email.Create($"owner_{Guid.NewGuid():N}@test.com"),
            passwordHash: "hashed",
            salt:         "salt",
            role:         "User");
        await _users.CreateAsync(user);
        return user;
    }
}
