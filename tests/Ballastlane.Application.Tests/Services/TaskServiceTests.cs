using Ballastlane.Application.DTOs;
using Ballastlane.Application.Ports;
using Ballastlane.Application.Services;
using Ballastlane.Domain.Entities;
using Ballastlane.Domain.Exceptions;

namespace Ballastlane.Application.Tests.Services;

/// <summary>
/// TDD tests for TaskService.
/// All dependencies are mocked with Moq → pure unit tests, zero I/O.
/// Pattern: Arrange / Act / Assert (AAA)
/// </summary>
public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _repoMock = new();
    private readonly TaskService _sut;

    public TaskServiceTests() => _sut = new TaskService(_repoMock.Object);

    // ── GetAllAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        var ownerId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            new(Guid.NewGuid(), "Task A", "Desc A", ownerId),
            new(Guid.NewGuid(), "Task B", "Desc B", ownerId),
        };
        _repoMock.Setup(r => r.ListAsync(default)).ReturnsAsync(tasks);

        var result = (await _sut.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, dto => Assert.IsType<TaskDto>(dto));
    }

    [Fact]
    public async Task GetAllAsync_EmptyList_ReturnsEmpty()
    {
        _repoMock.Setup(r => r.ListAsync(default)).ReturnsAsync(new List<TaskItem>());

        var result = await _sut.GetAllAsync();

        Assert.Empty(result);
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingTask_ReturnsDto()
    {
        var id = Guid.NewGuid();
        var task = new TaskItem(id, "My Task", "Desc", Guid.NewGuid());
        _repoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync(task);

        var dto = await _sut.GetByIdAsync(id);

        Assert.NotNull(dto);
        Assert.Equal(id, dto!.Id);
        Assert.Equal("My Task", dto.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
                 .ReturnsAsync((TaskItem?)null);

        var dto = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(dto);
    }

    // ── CreateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesAndReturnsDto()
    {
        var ownerId = Guid.NewGuid();
        var request = new CreateTaskRequest("Write tests", "TDD style", null);

        _repoMock.Setup(r => r.CreateAsync(It.IsAny<TaskItem>(), default))
                 .Returns(Task.CompletedTask);

        var dto = await _sut.CreateAsync(ownerId, request);

        Assert.Equal("Write tests", dto.Title);
        Assert.Equal("Todo", dto.Status);
        Assert.Equal(ownerId, dto.OwnerUserId);
        _repoMock.Verify(r => r.CreateAsync(It.IsAny<TaskItem>(), default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NullRequest_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _sut.CreateAsync(Guid.NewGuid(), null!));
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ExistingTask_UpdatesAndReturnsDto()
    {
        var id = Guid.NewGuid();
        var task = new TaskItem(id, "Old Title", "Old Desc", Guid.NewGuid());
        _repoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync(task);
        _repoMock.Setup(r => r.UpdateAsync(task, default)).Returns(Task.CompletedTask);

        var request = new UpdateTaskRequest("New Title", "New Desc", "InProgress", null);
        var dto = await _sut.UpdateAsync(id, request);

        Assert.Equal("New Title", dto.Title);
        Assert.Equal("InProgress", dto.Status);
        _repoMock.Verify(r => r.UpdateAsync(task, default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_TaskNotFound_ThrowsDomainException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
                 .ReturnsAsync((TaskItem?)null);

        var request = new UpdateTaskRequest("T", "D", "Todo", null);

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.UpdateAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task UpdateAsync_InvalidStatus_ThrowsDomainException()
    {
        var id = Guid.NewGuid();
        var task = new TaskItem(id, "Title", "Desc", Guid.NewGuid());
        _repoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync(task);

        var request = new UpdateTaskRequest("Title", "Desc", "INVALID_STATUS", null);

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.UpdateAsync(id, request));
    }

    // ── DeleteAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingTask_CallsRepository()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, default))
                 .ReturnsAsync(new TaskItem(id, "Task", "Desc", Guid.NewGuid()));
        _repoMock.Setup(r => r.DeleteAsync(id, default)).Returns(Task.CompletedTask);

        await _sut.DeleteAsync(id);

        _repoMock.Verify(r => r.DeleteAsync(id, default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_TaskNotFound_ThrowsDomainException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
                 .ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsAsync<DomainException>(() => _sut.DeleteAsync(Guid.NewGuid()));
    }
}
