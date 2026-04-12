using Ballastlane.Application.Ports;
using Ballastlane.Domain.Entities;

namespace Ballastlane.Api.Tests.Fakes;

/// <summary>
/// In-memory ITaskRepository test double — no SQL Server required.
/// Shared as a singleton within a test factory instance so all requests
/// in one test see the same data.
/// </summary>
public sealed class FakeTaskRepository : ITaskRepository
{
    private readonly List<TaskItem> _tasks = new();

    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_tasks.FirstOrDefault(t => t.Id == id));

    public Task<IEnumerable<TaskItem>> ListAsync(CancellationToken ct = default) =>
        Task.FromResult<IEnumerable<TaskItem>>(_tasks.ToList());

    public Task CreateAsync(TaskItem task, CancellationToken ct = default)
    {
        _tasks.Add(task);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TaskItem task, CancellationToken ct = default)
    {
        var idx = _tasks.FindIndex(t => t.Id == task.Id);
        if (idx >= 0) _tasks[idx] = task;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _tasks.RemoveAll(t => t.Id == id);
        return Task.CompletedTask;
    }

    /// <summary>Resets the store between test cases that share a factory.</summary>
    public void Clear() => _tasks.Clear();
}
