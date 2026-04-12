using Ballastlane.Domain.Entities;

namespace Ballastlane.Application.Ports;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<TaskItem>> ListAsync(CancellationToken ct = default);
    Task CreateAsync(TaskItem task, CancellationToken ct = default);
    Task UpdateAsync(TaskItem task, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
