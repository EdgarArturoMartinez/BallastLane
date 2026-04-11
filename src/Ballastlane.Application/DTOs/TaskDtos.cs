using Ballastlane.Domain.Entities;

namespace Ballastlane.Application.DTOs;

/// <summary>
/// Outbound DTO returned to callers — shields them from the domain entity.
/// Pattern: DTO (Data Transfer Object) — decouples API contract from domain model.
/// </summary>
public record TaskDto(
    Guid Id,
    string Title,
    string Description,
    string Status,
    DateTime? DueDate,
    Guid OwnerUserId);

/// <summary>Inbound request DTO for task creation.</summary>
public record CreateTaskRequest(
    string Title,
    string Description,
    DateTime? DueDate);

/// <summary>Inbound request DTO for task updates.</summary>
public record UpdateTaskRequest(
    string Title,
    string Description,
    string Status,
    DateTime? DueDate);

/// <summary>
/// Mapping helper — static factory on the DTO keeps entity clean (ISP).
/// SOLID: OCP — new fields can be added here without changing TaskItem.
/// </summary>
public static class TaskDtoMapper
{
    public static TaskDto ToDto(TaskItem task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Status.ToString(),
        task.DueDate,
        task.OwnerUserId);
}
