using Ballastlane.Application.Ports;
using Ballastlane.Infrastructure.Persistence.Repositories;

namespace Ballastlane.Infrastructure.Tests.Persistence;

/// <summary>
/// Integration tests for AdoAuditRepository.
/// Requires a running SQL Server — see DatabaseFixture.
/// </summary>
[Trait("Category", "Integration")]
[Collection("Database")]
public sealed class AdoAuditRepositoryIntegrationTests
{
    private readonly IAuditRepository _sut;

    public AdoAuditRepositoryIntegrationTests(DatabaseFixture db)
    {
        _sut = new AdoAuditRepository(db.Executor, db.Factory);
    }

    [Fact]
    public async Task InsertAsync_Then_ListAsync_Contains_Inserted_Record()
    {
        var entityId = Guid.NewGuid().ToString();

        await _sut.InsertAsync("Task", entityId, "Create", null, "testuser", null, new { Title = "x" });

        var list = await _sut.ListAsync();
        Assert.Contains(list, a => a.EntityId == entityId);
    }

    [Fact]
    public async Task ListPagedAsync_Returns_Correct_Page()
    {
        // Insert 3 known entries
        for (var i = 0; i < 3; i++)
            await _sut.InsertAsync("User", Guid.NewGuid().ToString(), "Login", null, $"paged_user_{i}", null, null);

        var page = await _sut.ListPagedAsync(1, 2, entity: "User");

        Assert.Equal(2, page.Items.Count());
        Assert.True(page.TotalCount >= 3);
    }

    [Fact]
    public async Task ListPagedAsync_FilterByAction_Returns_Only_Matching()
    {
        var uniqueEntityId = Guid.NewGuid().ToString();
        await _sut.InsertAsync("Task", uniqueEntityId, "Delete", null, "filteruser", null, null);

        var page = await _sut.ListPagedAsync(1, 50, action: "Delete");

        Assert.Contains(page.Items, a => a.EntityId == uniqueEntityId);
        Assert.All(page.Items, a => Assert.Equal("Delete", a.Action));
    }

    [Fact]
    public async Task ListDistinctEntitiesAsync_Contains_Inserted_Entity()
    {
        await _sut.InsertAsync("CustomEntity", Guid.NewGuid().ToString(), "Create", null, null, null, null);

        var entities = await _sut.ListDistinctEntitiesAsync();

        Assert.Contains("CustomEntity", entities);
    }

    [Fact]
    public async Task ListDistinctActionsAsync_Contains_Inserted_Action()
    {
        await _sut.InsertAsync("Task", Guid.NewGuid().ToString(), "CustomAction", null, null, null, null);

        var actions = await _sut.ListDistinctActionsAsync();

        Assert.Contains("CustomAction", actions);
    }
}
