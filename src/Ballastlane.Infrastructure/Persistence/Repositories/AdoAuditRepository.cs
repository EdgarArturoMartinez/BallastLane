using Ballastlane.Application.DTOs;
using Ballastlane.Application.Ports;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace Ballastlane.Infrastructure.Persistence.Repositories;

public sealed class AdoAuditRepository : IAuditRepository
{
    private readonly SqlConnectionFactory _factory;

    public AdoAuditRepository(SqlConnectionFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public async Task<IEnumerable<AuditDto>> ListAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, Entity, EntityId, Action, UserId, Username, OldValues, NewValues, CreatedAt
            FROM Audits
            ORDER BY CreatedAt DESC
            """;

        await using var conn = await _factory.CreateOpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var list = new List<AuditDto>();
        while (await reader.ReadAsync(ct))
        {
            var id = reader.GetGuid(0);
            var entity = reader.GetString(1);
            var entityId = reader.GetString(2);
            var action = reader.GetString(3);
            var userId = reader.IsDBNull(4) ? (Guid?)null : reader.GetGuid(4);
            var username = reader.IsDBNull(5) ? null : reader.GetString(5);
            var oldJson = reader.IsDBNull(6) ? null : reader.GetString(6);
            var newJson = reader.IsDBNull(7) ? null : reader.GetString(7);
            var createdAt = reader.GetDateTime(8);

            list.Add(AuditDtoMapper.FromReader(id, entity, entityId, action, userId, username, oldJson, newJson, createdAt));
        }

        return list;
    }

    public async Task<PagedResult<AuditDto>> ListPagedAsync(int page, int pageSize, string? entity = null, string? action = null, string? query = null, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var whereClauses = new List<string>();
        var parameters = new List<SqlParameter>();

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

        var where = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : string.Empty;

        var countSql = $"SELECT COUNT(1) FROM Audits {where}";
        await using var conn = await _factory.CreateOpenConnectionAsync(ct);
        await using var countCmd = new SqlCommand(countSql, conn);
        foreach (var p in parameters) countCmd.Parameters.Add(new SqlParameter(p.ParameterName, p.Value ?? DBNull.Value));
        var totalObj = await countCmd.ExecuteScalarAsync(ct);
        var total = Convert.ToInt32(totalObj ?? 0);

        var offset = (page - 1) * pageSize;
        var sql = $"SELECT Id, Entity, EntityId, Action, UserId, Username, OldValues, NewValues, CreatedAt FROM Audits {where} ORDER BY CreatedAt DESC OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

        await using var cmd = new SqlCommand(sql, conn);
        foreach (var p in parameters) cmd.Parameters.Add(new SqlParameter(p.ParameterName, p.Value ?? DBNull.Value));
        cmd.Parameters.AddWithValue("@offset", offset);
        cmd.Parameters.AddWithValue("@pageSize", pageSize);

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var list = new List<AuditDto>();
        while (await reader.ReadAsync(ct))
        {
            var id = reader.GetGuid(0);
            var entityName = reader.GetString(1);
            var entityId = reader.GetString(2);
            var actionName = reader.GetString(3);
            var userId = reader.IsDBNull(4) ? (Guid?)null : reader.GetGuid(4);
            var username = reader.IsDBNull(5) ? null : reader.GetString(5);
            var oldJson = reader.IsDBNull(6) ? null : reader.GetString(6);
            var newJson = reader.IsDBNull(7) ? null : reader.GetString(7);
            var createdAt = reader.GetDateTime(8);

            list.Add(AuditDtoMapper.FromReader(id, entityName, entityId, actionName, userId, username, oldJson, newJson, createdAt));
        }

        return new PagedResult<AuditDto>(list, total);
    }

    public async Task<IEnumerable<string>> ListDistinctEntitiesAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT DISTINCT Entity FROM Audits ORDER BY Entity";
        await using var conn = await _factory.CreateOpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var list = new List<string>();
        while (await reader.ReadAsync(ct))
        {
            list.Add(reader.GetString(0));
        }
        return list;
    }

    public async Task<IEnumerable<string>> ListDistinctActionsAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT DISTINCT Action FROM Audits ORDER BY Action";
        await using var conn = await _factory.CreateOpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var list = new List<string>();
        while (await reader.ReadAsync(ct))
        {
            list.Add(reader.GetString(0));
        }
        return list;
    }

    public async Task InsertAsync(
        string entity,
        string entityId,
        string action,
        Guid? userId,
        string? username,
        object? oldValues,
        object? newValues,
        CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO Audits (Entity, EntityId, Action, UserId, Username, OldValues, NewValues)
            VALUES (@entity, @entityId, @action, @userId, @username, @oldValues, @newValues)
            """;

        var oldJson = oldValues is null ? null : JsonSerializer.Serialize(oldValues);
        var newJson = newValues is null ? null : JsonSerializer.Serialize(newValues);

        await using var conn = await _factory.CreateOpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@entity", entity);
        cmd.Parameters.AddWithValue("@entityId", entityId);
        cmd.Parameters.AddWithValue("@action", action);
        cmd.Parameters.AddWithValue("@userId", (object?)userId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@username", (object?)username ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@oldValues", (object?)oldJson ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@newValues", (object?)newJson ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync(ct);
    }
}
