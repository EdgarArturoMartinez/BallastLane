using Ballastlane.Application.Ports;
using Ballastlane.Domain.Entities;
using Microsoft.Data.SqlClient;
using Ballastlane.Infrastructure.SqlQueries;

namespace Ballastlane.Infrastructure.Persistence.Repositories;

/// <summary>
/// ADO.NET adapter implementing ITaskRepository.
///
/// Pattern: Repository (secondary adapter in Hexagonal arch).
/// SQL text is loaded from SqlQueries/*.sql (centralized, versionable).
/// Execution is delegated to DbExecutor — no ADO.NET boilerplate here.
///
/// SOLID:
///   SRP  — only handles Task persistence; no business logic.
///   DIP  — implements the ITaskRepository port defined in Application.
///   LSP  — fully substitutable for any ITaskRepository consumer (including mocks in tests).
///
/// All SQL uses parameterized queries — OWASP SQL-Injection safe.
/// </summary>
public sealed class AdoTaskRepository : ITaskRepository
{
    private readonly DbExecutor _db;

    public AdoTaskRepository(DbExecutor db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.QuerySingleOrDefaultAsync(
            SqlQueryLoader.Get("Tasks.GetById"),
            cmd => cmd.Parameters.AddWithValue("@id", id),
            MapTask,
            ct);

    public Task<IEnumerable<TaskItem>> ListAsync(CancellationToken ct = default) =>
        _db.QueryAsync(
            SqlQueryLoader.Get("Tasks.List"),
            _ => { },
            MapTask,
            ct);

    public Task CreateAsync(TaskItem task, CancellationToken ct = default) =>
        _db.ExecuteAsync(
            SqlQueryLoader.Get("Tasks.Create"),
            cmd =>
            {
                cmd.Parameters.AddWithValue("@id",          task.Id);
                cmd.Parameters.AddWithValue("@title",       task.Title);
                cmd.Parameters.AddWithValue("@description", task.Description);
                cmd.Parameters.AddWithValue("@status",      task.Status.ToString());
                cmd.Parameters.AddWithValue("@dueDate",     (object?)task.DueDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ownerUserId", task.OwnerUserId);
            },
            ct);

    public Task UpdateAsync(TaskItem task, CancellationToken ct = default) =>
        _db.ExecuteAsync(
            SqlQueryLoader.Get("Tasks.Update"),
            cmd =>
            {
                cmd.Parameters.AddWithValue("@id",          task.Id);
                cmd.Parameters.AddWithValue("@title",       task.Title);
                cmd.Parameters.AddWithValue("@description", task.Description);
                cmd.Parameters.AddWithValue("@status",      task.Status.ToString());
                cmd.Parameters.AddWithValue("@dueDate",     (object?)task.DueDate ?? DBNull.Value);
            },
            ct);

    public Task DeleteAsync(Guid id, CancellationToken ct = default) =>
        _db.ExecuteAsync(
            SqlQueryLoader.Get("Tasks.Delete"),
            cmd => cmd.Parameters.AddWithValue("@id", id),
            ct);

    // ── Mapping ────────────────────────────────────────────────────────────

    private static TaskItem MapTask(SqlDataReader r)
    {
        var id          = r.GetGuid(0);
        var title       = r.GetString(1);
        var description = r.GetString(2);
        var statusStr   = r.GetString(3);
        var dueDate     = r.IsDBNull(4) ? (DateTime?)null : r.GetDateTime(4);
        var ownerId     = r.GetGuid(5);

        var status = Enum.Parse<TaskItemStatus>(statusStr, ignoreCase: true);

        var task = new TaskItem(id, title, description, ownerId, dueDate);

        // Apply persisted status (default is Todo; skip update if already correct)
        if (status != TaskItemStatus.Todo)
            task.Update(title, description, status, dueDate);

        return task;
    }
}
