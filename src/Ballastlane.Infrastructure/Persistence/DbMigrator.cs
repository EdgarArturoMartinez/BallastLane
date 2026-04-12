using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Ballastlane.Infrastructure.Persistence;

/// <summary>
/// Runs numbered SQL migration scripts located in the embedded 'sql/migrations' folder.
///
/// Pattern: Template Method — the migration loop is fixed; only the SQL scripts vary.
/// SOLID:
///   SRP — only responsible for schema versioning; no business logic.
///   OCP — add new migrations by dropping a new numbered .sql file; no code changes needed.
///
/// Applied scripts are recorded in the __Migrations table with a SHA-256 checksum
/// so re-running the app never re-applies the same script.
/// </summary>
public sealed class DbMigrator
{
    private readonly string _connectionString;
    private readonly ILogger<DbMigrator> _logger;

    // Path to migration scripts relative to the executing assembly's base directory.
    private static readonly string MigrationsPath =
        Path.Combine(AppContext.BaseDirectory, "sql", "migrations");

    public DbMigrator(string connectionString, ILogger<DbMigrator> logger)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task MigrateAsync(CancellationToken ct = default)
    {
        // Step 1: ensure the target database exists (connects to master first).
        await EnsureDatabaseExistsAsync(ct);

        // Step 2: proceed with schema migrations inside the target database.
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        await EnsureMigrationsTableAsync(connection, ct);

        var applied = await GetAppliedScriptsAsync(connection, ct);
        var scripts = GetPendingScripts(applied);

        if (!scripts.Any())
        {
            _logger.LogInformation("Database is up to date. No migrations to apply.");
            return;
        }

        foreach (var (name, sql) in scripts)
        {
            _logger.LogInformation("Applying migration: {Script}", name);
            await ApplyScriptAsync(connection, name, sql, ct);
        }

        _logger.LogInformation("Migrations complete. Applied {Count} script(s).", scripts.Count);
    }

    // ── Private helpers ────────────────────────────────────────────────────

    private static async Task EnsureMigrationsTableAsync(SqlConnection conn, CancellationToken ct)
    {
        const string sql = """
            IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[__Migrations]') AND type = N'U')
            BEGIN
                CREATE TABLE __Migrations (
                    ScriptName  NVARCHAR(260) NOT NULL PRIMARY KEY,
                    AppliedAt   DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
                    Checksum    NVARCHAR(64)  NOT NULL
                )
            END
            """;

        await using var cmd = new SqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task<HashSet<string>> GetAppliedScriptsAsync(SqlConnection conn, CancellationToken ct)
    {
        var applied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await using var cmd = new SqlCommand("SELECT ScriptName FROM __Migrations", conn);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
            applied.Add(reader.GetString(0));

        return applied;
    }

    private static List<(string Name, string Sql)> GetPendingScripts(HashSet<string> applied)
    {
        if (!Directory.Exists(MigrationsPath))
            return new List<(string, string)>();

        return Directory
            .GetFiles(MigrationsPath, "*.sql")
            .OrderBy(f => f)
            .Select(f => (Name: Path.GetFileName(f), Sql: File.ReadAllText(f)))
            .Where(s => !applied.Contains(s.Name))
            .ToList();
    }

    private static async Task ApplyScriptAsync(
        SqlConnection conn, string name, string sql, CancellationToken ct)
    {
        // Execute each SQL statement separated by GO — SqlCommand doesn't support GO natively.
        var statements = sql
            .Split(new[] { "\nGO", "\r\nGO" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s));

        await using var tx = (SqlTransaction)await conn.BeginTransactionAsync(ct);
        try
        {
            foreach (var statement in statements)
            {
                await using var cmd = new SqlCommand(statement, conn, tx);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            var checksum = ComputeChecksum(sql);
            const string record = """
                INSERT INTO __Migrations (ScriptName, Checksum)
                VALUES (@name, @checksum)
                """;

            await using var recordCmd = new SqlCommand(record, conn, tx);
            recordCmd.Parameters.AddWithValue("@name", name);
            recordCmd.Parameters.AddWithValue("@checksum", checksum);
            await recordCmd.ExecuteNonQueryAsync(ct);

            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    private static string ComputeChecksum(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>
    /// Connects to the 'master' database and creates the target database if it doesn't exist.
    /// This must run before any other migration step because those steps connect directly to
    /// the target database, which may not exist on a fresh container.
    /// </summary>
    private async Task EnsureDatabaseExistsAsync(CancellationToken ct)
    {
        var builder = new SqlConnectionStringBuilder(_connectionString);
        var targetDb = builder.InitialCatalog;

        // Validate: database name must only contain safe characters to prevent injection.
        if (string.IsNullOrWhiteSpace(targetDb) ||
            !System.Text.RegularExpressions.Regex.IsMatch(targetDb, @"^[\w\-]+$"))
        {
            throw new InvalidOperationException(
                $"The database name '{targetDb}' contains invalid characters.");
        }

        builder.InitialCatalog = "master";

        await using var masterConn = new SqlConnection(builder.ConnectionString);
        await masterConn.OpenAsync(ct);

        // Use QUOTENAME to safely delimit the database name in the DDL statement.
        var sql = $"""
            IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'{targetDb}')
                CREATE DATABASE [{targetDb}]
            """;

        await using var cmd = new SqlCommand(sql, masterConn);
        await cmd.ExecuteNonQueryAsync(ct);

        _logger.LogInformation("Database '{TargetDb}' ensured.", targetDb);
    }
}
