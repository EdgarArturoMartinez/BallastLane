using Ballastlane.Domain.Entities;
using Ballastlane.Domain.Exceptions;

namespace Ballastlane.Domain.Tests.Entities;

/// <summary>
/// TDD tests for TaskItem entity.
/// These tests were written BEFORE the entity was finalized — classic Red-Green-Refactor cycle.
/// </summary>
public class TaskItemTests
{
    private readonly Guid _validId = Guid.NewGuid();
    private readonly Guid _validOwner = Guid.NewGuid();

    // ── Construction ──────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ValidArgs_CreatesTaskWithTodoStatus()
    {
        var task = new TaskItem(_validId, "Buy milk", "From the shop", _validOwner);

        Assert.Equal(_validId, task.Id);
        Assert.Equal("Buy milk", task.Title);
        Assert.Equal("From the shop", task.Description);
        Assert.Equal(TaskItemStatus.Todo, task.Status);
        Assert.Equal(_validOwner, task.OwnerUserId);
        Assert.Null(task.DueDate);
    }

    [Fact]
    public void Constructor_WithDueDate_SetsDueDate()
    {
        var due = new DateTime(2026, 12, 31);
        var task = new TaskItem(_validId, "Yearly report", "", _validOwner, due);

        Assert.Equal(due, task.DueDate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_EmptyTitle_ThrowsDomainException(string? title)
    {
        Assert.Throws<DomainException>(() =>
            new TaskItem(_validId, title!, "desc", _validOwner));
    }

    [Fact]
    public void Constructor_EmptyId_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new TaskItem(Guid.Empty, "Title", "desc", _validOwner));
    }

    [Fact]
    public void Constructor_EmptyOwner_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new TaskItem(_validId, "Title", "desc", Guid.Empty));
    }

    [Fact]
    public void Constructor_NullDescription_SetsEmptyString()
    {
        var task = new TaskItem(_validId, "Title", null!, _validOwner);

        Assert.Equal(string.Empty, task.Description);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public void Update_ValidArgs_UpdatesAllFields()
    {
        var task = new TaskItem(_validId, "Old title", "Old desc", _validOwner);
        var newDue = new DateTime(2026, 6, 1);

        task.Update("New title", "New desc", TaskItemStatus.InProgress, newDue);

        Assert.Equal("New title", task.Title);
        Assert.Equal("New desc", task.Description);
        Assert.Equal(TaskItemStatus.InProgress, task.Status);
        Assert.Equal(newDue, task.DueDate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_EmptyTitle_ThrowsDomainException(string badTitle)
    {
        var task = new TaskItem(_validId, "Title", "desc", _validOwner);

        Assert.Throws<DomainException>(() =>
            task.Update(badTitle, "desc", TaskItemStatus.Done, null));
    }

    [Fact]
    public void Update_ClearsTitle_Whitespace_IsTrimmed()
    {
        var task = new TaskItem(_validId, "  Title  ", "desc", _validOwner);

        Assert.Equal("Title", task.Title);
    }
}
