using Microsoft.Data.SqlClient;

namespace Ballastlane.Infrastructure.Persistence;

/// <summary>
/// Centralizes ADO.NET command execution, eliminating boilerplate from repositories.
///
/// Design:
///   - Receives SQL text + a parameter-binding callback + a row-mapper.
///   - Repositories remain focused on SQL selection and row mapping only.
///   - All connections are opened via SqlConnectionFactory (no leaked connections).
///   - All queries are parameterized — OWASP SQL-Injection safe.
///
/// SOLID compliance:
///   SRP  — only handles low-level command execution.
///   DIP  — depends on SqlConnectionFactory (injected abstraction).
///   OCP  — new query shapes can be added without modifying existing methods.
/// </summary>
public sealed class DbExecutor
{
    private readonly SqlConnectionFactory _factory;

    public DbExecutor(SqlConnectionFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>Executes a SELECT and maps every row to <typeparamref name="T"/>.</summary>
    public async Task<IEnumerable<T>> QueryAsync<T>(
        string sql,
        Action<SqlCommand> addParams,
        Func<SqlDataReader, T> map,
        CancellationToken ct = default)
    {
        await using var conn = await _factory.CreateOpenConnectionAsync(ct);
        await using var cmd  = new SqlCommand(sql, conn);
        addParams(cmd);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var results = new List<T>();
        while (await reader.ReadAsync(ct))
            results.Add(map(reader));

        return results;
    }

    /// <summary>Executes a SELECT and maps the first row, or returns <c>null</c>.</summary>
    public async Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        Action<SqlCommand> addParams,
        Func<SqlDataReader, T> map,
        CancellationToken ct = default) where T : class
    {
        await using var conn = await _factory.CreateOpenConnectionAsync(ct);
        await using var cmd  = new SqlCommand(sql, conn);
        addParams(cmd);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? map(reader) : null;
    }

    /// <summary>Executes an INSERT / UPDATE / DELETE. Returns rows affected.</summary>
    public async Task<int> ExecuteAsync(
        string sql,
        Action<SqlCommand> addParams,
        CancellationToken ct = default)
    {
        await using var conn = await _factory.CreateOpenConnectionAsync(ct);
        await using var cmd  = new SqlCommand(sql, conn);
        addParams(cmd);

        return await cmd.ExecuteNonQueryAsync(ct);
    }

    /// <summary>Executes a scalar query (e.g. COUNT) on an already-open connection.</summary>
    public static async Task<T?> ExecuteScalarOnAsync<T>(
        SqlConnection conn,
        string sql,
        Action<SqlCommand> addParams,
        CancellationToken ct = default)
    {
        await using var cmd = new SqlCommand(sql, conn);
        addParams(cmd);

        var result = await cmd.ExecuteScalarAsync(ct);
        if (result is null or DBNull) return default;
        return (T)Convert.ChangeType(result, typeof(T));
    }

    /// <summary>Executes a SELECT on an already-open connection (used for multi-step queries).</summary>
    public static async Task<IEnumerable<T>> QueryOnAsync<T>(
        SqlConnection conn,
        string sql,
        Action<SqlCommand> addParams,
        Func<SqlDataReader, T> map,
        CancellationToken ct = default)
    {
        await using var cmd = new SqlCommand(sql, conn);
        addParams(cmd);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var results = new List<T>();
        while (await reader.ReadAsync(ct))
            results.Add(map(reader));

        return results;
    }
}
