using Ballastlane.Application.Ports;
using Ballastlane.Domain.Entities;
using Microsoft.Data.SqlClient;
using Ballastlane.Infrastructure.SqlQueries;

namespace Ballastlane.Infrastructure.Persistence.Repositories;

/// <summary>
/// ADO.NET adapter implementing ITaskRepository.
///
/// Pattern: Repository (secondary adapter in Hexagonal arch).
/// SOLID:
///   SRP  — only handles Task persistence; no business logic.
///   DIP  — implements the ITaskRepository port defined in Application.
///   LSP  — fully substitutable for any ITaskRepository consumer (including mocks in tests).
///
/// All SQL uses parameterized queries — no string concatenation → OWASP SQL-Injection safe.
/// </summary>
public sealed class AdoTaskRepository : ITaskRepository
{
    private readonly SqlConnectionFactory _factory;

    public AdoTaskRepository(SqlConnectionFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var sql = SqlQueryLoader.Get("Tasks.GetById");

        await using var conn = await _factory.CreateOpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? MapTask(reader) : null;
    }

    public async Task<IEnumerable<TaskItem>> ListAsync(CancellationToken ct = default)
    {
        var sql = SqlQueryLoader.Get("Tasks.List");

        await using var conn = await _factory.CreateOpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var tasks = new List<TaskItem>();
        while (await reader.ReadAsync(ct))
            tasks.Add(MapTask(reader));

        return tasks;
    }

    public async Task CreateAsync(TaskItem task, CancellationToken ct = default)
    {
        var sql = SqlQueryLoader.Get("Tasks.Create");

        await using var conn = await _factory.CreateOpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id",          task.Id);
        cmd.Parameters.AddWithValue("@title",       task.Title);
        cmd.Parameters.AddWithValue("@description", task.Description);
        cmd.Parameters.AddWithValue("@status",      task.Status.ToString());
        cmd.Parameters.AddWithValue("@dueDate",     (object?)task.DueDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ownerUserId", task.OwnerUserId);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken ct = default)
    {
        var sql = SqlQueryLoader.Get("Tasks.Update");

        await using var conn = await _factory.CreateOpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id",          task.Id);
        cmd.Parameters.AddWithValue("@title",       task.Title);
        cmd.Parameters.AddWithValue("@description", task.Description);
        cmd.Parameters.AddWithValue("@status",      task.Status.ToString());
        cmd.Parameters.AddWithValue("@dueDate",     (object?)task.DueDate ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var sql = SqlQueryLoader.Get("Tasks.Delete");

        await using var conn = await _factory.CreateOpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        await cmd.ExecuteNonQueryAsync(ct);
    }

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
