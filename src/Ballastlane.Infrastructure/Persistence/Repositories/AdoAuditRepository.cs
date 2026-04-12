using Ballastlane.Application.DTOs;
using Ballastlane.Application.Ports;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using Ballastlane.Infrastructure.SqlQueries;

namespace Ballastlane.Infrastructure.Persistence.Repositories;

/// <summary>
/// ADO.NET adapter implementing IAuditRepository.
/// Static queries loaded from SqlQueries/*.sql.
/// Dynamic paged query (ListPagedAsync) builds WHERE clauses at runtime — still fully parameterized.
/// Execution delegated to DbExecutor.
/// </summary>
public sealed class AdoAuditRepository : IAuditRepository
{
    private readonly DbExecutor _db;
    private readonly SqlConnectionFactory _factory;

    public AdoAuditRepository(DbExecutor db, SqlConnectionFactory factory)
    {
        _db      = db      ?? throw new ArgumentNullException(nameof(db));
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public Task<IEnumerable<AuditDto>> ListAsync(CancellationToken ct = default) =>
        _db.QueryAsync(
            SqlQueryLoader.Get("Audits.List"),
            _ => { },
            MapAudit,
            ct);

    public async Task<PagedResult<AuditDto>> ListPagedAsync(
        int page, int pageSize,
        string? entity = null, string? action = null, string? query = null,
        CancellationToken ct = default)
    {
        if (page < 1)     page     = 1;
        if (pageSize < 1) pageSize = 10;

        var whereClauses = new List<string>();
        var parameters   = new List<SqlParameter>();

        if (!string.IsNullOrWhiteSpace(entity) && entity != "All")
        {
            whereClauses.Add("Entity = @entity");
            parameters.Add(new SqlParameter("@entity", entity));
        }
        if (!string.IsNullOrWhiteSpace(action) && action != "All")
        {
            whereClauses.Add("Action = @action");
            parameters.Add(new SqlParameter("@action", action));
        }
        if (!string.IsNullOrWhiteSpace(query))
        {
            whereClauses.Add("(Entity LIKE @q OR Username LIKE @q OR EntityId LIKE @q)");
            parameters.Add(new SqlParameter("@q", "%" + query + "%"));
        }

        var where    = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : string.Empty;
        var countSql = $"SELECT COUNT(1) FROM Audits {where}";
        var dataSql  = $"SELECT Id, Entity, EntityId, Action, UserId, Username, OldValues, NewValues, CreatedAt " +
                       $"FROM Audits {where} ORDER BY CreatedAt DESC " +
                       $"OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

        await using var conn = await _factory.CreateOpenConnectionAsync(ct);

        var total = await DbExecutor.ExecuteScalarOnAsync<int>(
            conn, countSql,
            cmd => { foreach (var p in parameters) cmd.Parameters.Add(Clone(p)); },
            ct);

        var offset = (page - 1) * pageSize;
        var list   = await DbExecutor.QueryOnAsync(
            conn, dataSql,
            cmd =>
            {
                foreach (var p in parameters) cmd.Parameters.Add(Clone(p));
                cmd.Parameters.AddWithValue("@offset",   offset);
                cmd.Parameters.AddWithValue("@pageSize", pageSize);
            },
            MapAudit,
            ct);

        return new PagedResult<AuditDto>(list, total);
    }

    public Task<IEnumerable<string>> ListDistinctEntitiesAsync(CancellationToken ct = default) =>
        _db.QueryAsync(
            SqlQueryLoader.Get("Audits.DistinctEntities"),
            _ => { },
            r => r.GetString(0),
            ct);

    public Task<IEnumerable<string>> ListDistinctActionsAsync(CancellationToken ct = default) =>
        _db.QueryAsync(
            SqlQueryLoader.Get("Audits.DistinctActions"),
            _ => { },
            r => r.GetString(0),
            ct);

    public Task InsertAsync(
        string entity, string entityId, string action,
        Guid? userId, string? username,
        object? oldValues, object? newValues,
        CancellationToken ct = default)
    {
        var oldJson = oldValues is null ? null : JsonSerializer.Serialize(oldValues);
        var newJson = newValues is null ? null : JsonSerializer.Serialize(newValues);

        return _db.ExecuteAsync(
            SqlQueryLoader.Get("Audits.Insert"),
            cmd =>
            {
                cmd.Parameters.AddWithValue("@entity",    entity);
                cmd.Parameters.AddWithValue("@entityId",  entityId);
                cmd.Parameters.AddWithValue("@action",    action);
                cmd.Parameters.AddWithValue("@userId",    (object?)userId    ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@username",  (object?)username  ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@oldValues", (object?)oldJson   ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@newValues", (object?)newJson   ?? DBNull.Value);
            },
            ct);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    // SqlParameter cannot be added to two commands; clone for reuse across count/data commands.
    private static SqlParameter Clone(SqlParameter p) =>
        new(p.ParameterName, p.Value ?? DBNull.Value);

    // ── Mapping ────────────────────────────────────────────────────────────

    private static AuditDto MapAudit(SqlDataReader r) =>
        AuditDtoMapper.FromReader(
            id:        r.GetGuid(0),
            entity:    r.GetString(1),
            entityId:  r.GetString(2),
            action:    r.GetString(3),
            userId:    r.IsDBNull(4) ? (Guid?)null : r.GetGuid(4),
            username:  r.IsDBNull(5) ? null : r.GetString(5),
            oldJson:   r.IsDBNull(6) ? null : r.GetString(6),
            newJson:   r.IsDBNull(7) ? null : r.GetString(7),
            createdAt: r.GetDateTime(8));
}
