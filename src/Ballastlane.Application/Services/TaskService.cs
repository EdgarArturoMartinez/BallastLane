using Ballastlane.Application.DTOs;
using Ballastlane.Application.Ports;
using Ballastlane.Domain.Entities;
using Ballastlane.Domain.Exceptions;

namespace Ballastlane.Application.Services;

/// <summary>
/// Application service — implements CRUD use-cases for TaskItem.
///
/// SOLID:
///   SRP — only orchestrates task-related use-cases; no auth, no HTTP.
///   DIP — depends on ITaskRepository port; infra layer provides the implementation.
///   OCP — new use-cases added by extending ITaskService, not modifying this class.
///
/// Pattern: Service Layer (application / use-case layer in Hexagonal arch).
/// TDD: tests for this service were written first (see Ballastlane.Application.Tests).
/// </summary>
public sealed class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
    }

    public async Task<IEnumerable<TaskDto>> GetAllAsync(CancellationToken ct = default)
    {
        var tasks = await _taskRepository.ListAsync(ct);
        return tasks.Select(TaskDtoMapper.ToDto);
    }

    public async Task<TaskDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, ct);
        return task is null ? null : TaskDtoMapper.ToDto(task);
    }

    public async Task<TaskDto> CreateAsync(Guid ownerId, CreateTaskRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var task = new TaskItem(
            Guid.NewGuid(),
            request.Title,
            request.Description,
            ownerId,
            request.DueDate);

        await _taskRepository.CreateAsync(task, ct);
        return TaskDtoMapper.ToDto(task);
    }

    public async Task<TaskDto> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var task = await _taskRepository.GetByIdAsync(id, ct)
            ?? throw new DomainException($"Task '{id}' not found.");

        if (!Enum.TryParse<TaskItemStatus>(request.Status, ignoreCase: true, out var status))
            throw new DomainException($"Unknown status '{request.Status}'.");

        task.Update(request.Title, request.Description, status, request.DueDate);
        await _taskRepository.UpdateAsync(task, ct);
        return TaskDtoMapper.ToDto(task);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, ct)
            ?? throw new DomainException($"Task '{id}' not found.");

        await _taskRepository.DeleteAsync(task.Id, ct);
    }
}
