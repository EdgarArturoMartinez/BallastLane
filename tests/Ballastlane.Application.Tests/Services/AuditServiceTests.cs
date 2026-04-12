using Ballastlane.Application.DTOs;
using Ballastlane.Application.Ports;
using Ballastlane.Application.Services;

namespace Ballastlane.Application.Tests.Services;

public class AuditServiceTests
{
    private readonly Mock<IAuditRepository> _repoMock = new();
    private readonly AuditService _sut;

    public AuditServiceTests()
    {
        _sut = new AuditService(_repoMock.Object);
    }

    // ── GetAllAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllRecordsFromRepository()
    {
        var records = new List<AuditDto>
        {
            new(Guid.NewGuid(), "Users", "u1", "Created", null, "alice", null, null, DateTime.UtcNow),
            new(Guid.NewGuid(), "Tasks", "t1", "Deleted", null, "bob",   null, null, DateTime.UtcNow),
        };

        _repoMock.Setup(r => r.ListAsync(default)).ReturnsAsync(records);

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_EmptyRepo_ReturnsEmptyEnumerable()
    {
        _repoMock.Setup(r => r.ListAsync(default))
                 .ReturnsAsync(Enumerable.Empty<AuditDto>());

        var result = await _sut.GetAllAsync();

        Assert.Empty(result);
    }

    // ── GetPagedAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetPagedAsync_DelegatesToRepository_WithAllParameters()
    {
        var expected = new PagedResult<AuditDto>(Enumerable.Empty<AuditDto>(), 0);

        _repoMock
            .Setup(r => r.ListPagedAsync(2, 5, "Tasks", "Deleted", "q", default))
            .ReturnsAsync(expected);

        var result = await _sut.GetPagedAsync(2, 5, "Tasks", "Deleted", "q");

        Assert.Same(expected, result);
        _repoMock.Verify(r => r.ListPagedAsync(2, 5, "Tasks", "Deleted", "q", default), Times.Once);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsCorrectTotalCount()
    {
        var items = new List<AuditDto>
        {
            new(Guid.NewGuid(), "Users", "u1", "Created", null, "alice", null, null, DateTime.UtcNow)
        };
        var pagedResult = new PagedResult<AuditDto>(items, 42);

        _repoMock
            .Setup(r => r.ListPagedAsync(1, 10, null, null, null, default))
            .ReturnsAsync(pagedResult);

        var result = await _sut.GetPagedAsync(1, 10);

        Assert.Equal(42, result.TotalCount);
        Assert.Single(result.Items);
    }

    // ── GetMetadataAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetMetadataAsync_ReturnsCombinedEntitiesAndActions()
    {
        _repoMock.Setup(r => r.ListDistinctEntitiesAsync(default))
                 .ReturnsAsync(new[] { "Users", "Tasks" });
        _repoMock.Setup(r => r.ListDistinctActionsAsync(default))
                 .ReturnsAsync(new[] { "Created", "Deleted", "Updated" });

        var (entities, actions) = await _sut.GetMetadataAsync();

        Assert.Equal(new[] { "Users", "Tasks" }, entities);
        Assert.Equal(new[] { "Created", "Deleted", "Updated" }, actions);
    }

    [Fact]
    public async Task GetMetadataAsync_CallsBothRepositoryMethods()
    {
        _repoMock.Setup(r => r.ListDistinctEntitiesAsync(default))
                 .ReturnsAsync(Enumerable.Empty<string>());
        _repoMock.Setup(r => r.ListDistinctActionsAsync(default))
                 .ReturnsAsync(Enumerable.Empty<string>());

        await _sut.GetMetadataAsync();

        _repoMock.Verify(r => r.ListDistinctEntitiesAsync(default), Times.Once);
        _repoMock.Verify(r => r.ListDistinctActionsAsync(default), Times.Once);
    }

    // ── Constructor ──────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new AuditService(null!));
    }
}
