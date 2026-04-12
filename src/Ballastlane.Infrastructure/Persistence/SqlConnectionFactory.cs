using Microsoft.Data.SqlClient;

namespace Ballastlane.Infrastructure.Persistence;

/// <summary>
/// Provides a SQL connection factory used by all repository adapters.
/// Pattern: Factory (simple) — repositories don't own connection lifecycle.
/// SOLID: SRP — single responsibility: create open connections.
/// </summary>
public sealed class SqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required.", nameof(connectionString));

        _connectionString = connectionString;
    }

    public async Task<SqlConnection> CreateOpenConnectionAsync(CancellationToken ct = default)
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);
        return connection;
    }
}
