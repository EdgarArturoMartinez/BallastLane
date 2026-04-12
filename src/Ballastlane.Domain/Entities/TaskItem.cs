using Ballastlane.Domain.Exceptions;

namespace Ballastlane.Domain.Entities;

/// <summary>
/// Enum: TaskItemStatus — uses 'TaskItem' prefix to avoid ambiguity with System.Threading.Tasks.TaskStatus.
/// </summary>
public enum TaskItemStatus { Todo, InProgress, Done }

/// <summary>
/// Aggregate root — TaskItem.
/// Pattern: Entity with private setters; invariants enforced in constructor and mutation methods.
/// SOLID:
///   SRP  — holds task state only; no persistence or HTTP concerns.
///   OCP  — new statuses can be added to the enum without changing this class.
///   DIP  — depends on DomainException abstraction, not on infrastructure.
/// </summary>
public class TaskItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public DateTime? DueDate { get; private set; }
    public Guid OwnerUserId { get; private set; }

    public TaskItem(
        Guid id,
        string title,
        string description,
        Guid ownerUserId,
        DateTime? dueDate = null)
    {
        if (id == Guid.Empty)
            throw new DomainException("Task id must not be empty.");
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");
        if (ownerUserId == Guid.Empty)
            throw new DomainException("OwnerUserId must not be empty.");

        Id = id;
        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        OwnerUserId = ownerUserId;
        DueDate = dueDate;
        Status = TaskItemStatus.Todo;
    }

    /// <summary>Domain operation — updates all mutable fields at once.</summary>
    public void Update(string title, string description, TaskItemStatus status, DateTime? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        Status = status;
        DueDate = dueDate;
    }
}
