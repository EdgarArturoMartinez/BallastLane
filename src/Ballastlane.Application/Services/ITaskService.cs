using Ballastlane.Application.DTOs;

namespace Ballastlane.Application.Services;

/// <summary>
/// Port (primary): CRUD use-cases for tasks.
/// SOLID: ISP — one focused interface per domain use-case group.
/// </summary>
public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetAllAsync(CancellationToken ct = default);
    Task<TaskDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TaskDto> CreateAsync(Guid ownerId, CreateTaskRequest request, CancellationToken ct = default);
    Task<TaskDto> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
