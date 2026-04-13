using Ballastlane.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;

namespace Ballastlane.Infrastructure.Tests.Persistence;

/// <summary>
/// xUnit class fixture that connects to a SQL Server instance and
/// bootstraps a clean, isolated test database before the test collection runs.
///
/// Connection string resolution order:
///   1. Environment variable  SQLSERVER_INTEGRATION_CONNSTR
///   2. Local Docker default  (SA password matching docker-compose.yml)
///
/// The fixture creates a short-lived database (IntegrationTest_<guid>), applies
/// all migrations via DbMigrator, and drops the database on dispose.
/// This guarantees test isolation without touching the development database.
/// </summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
    private const string MigrationsPath = "../../../../sql/migrations";

    private readonly string _masterConnStr;
    private string _dbName = null!;

    public SqlConnectionFactory Factory { get; private set; } = null!;
    public DbExecutor           Executor { get; private set; } = null!;

    public DatabaseFixture()
    {
        _masterConnStr = Environment.GetEnvironmentVariable("SQLSERVER_INTEGRATION_CONNSTR");
        if (string.IsNullOrWhiteSpace(_masterConnStr))
            throw new InvalidOperationException(
                "SQLSERVER_INTEGRATION_CONNSTR is not set. Set it to a valid SQL Server connection string (see .env.example). To run integration tests locally you can set SQLSERVER_INTEGRATION_CONNSTR or set SA_PASSWORD and use docker-compose with a .env file.");
    }

    public async Task InitializeAsync()
    {
        _dbName = $"IntegrationTest_{Guid.NewGuid():N}";

        // Create isolated database
        await using var conn = new SqlConnection(_masterConnStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand($"CREATE DATABASE [{_dbName}]", conn);
        await cmd.ExecuteNonQueryAsync();

        var testConnStr = BuildTestConnStr();
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<DbMigrator>.Instance;
        var migrator = new DbMigrator(testConnStr, logger);
        await migrator.MigrateAsync();

        Factory  = new SqlConnectionFactory(testConnStr);
        Executor = new DbExecutor(Factory);
    }

    public async Task DisposeAsync()
    {
        await using var conn = new SqlConnection(_masterConnStr);
        await conn.OpenAsync();
        // Force close any open connections, then drop
        await using var cmd = new SqlCommand(
            $"ALTER DATABASE [{_dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{_dbName}]",
            conn);
        await cmd.ExecuteNonQueryAsync();
    }

    private string BuildTestConnStr()
    {
        var builder = new SqlConnectionStringBuilder(_masterConnStr) { InitialCatalog = _dbName };
        return builder.ConnectionString;
    }
}
