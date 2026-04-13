using Ballastlane.Infrastructure.Persistence;

namespace Ballastlane.Infrastructure.Tests.Persistence;

/// <summary>
/// Unit tests for DbExecutor.
/// Real query execution requires a live SQL Server — that is covered by
/// the integration tests. This file tests the public contract and guard clauses
/// that can be verified without a database connection.
/// </summary>
public sealed class DbExecutorTests
{
    // ── Constructor guards ───────────────────────────────────────────────

    [Fact]
    public void Constructor_NullFactory_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new DbExecutor(null!));

        Assert.Equal("factory", ex.ParamName);
    }

    // ── SqlConnectionFactory guards ──────────────────────────────────────

    [Fact]
    public void SqlConnectionFactory_NullConnectionString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new SqlConnectionFactory(null!));
    }

    [Fact]
    public void SqlConnectionFactory_EmptyConnectionString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new SqlConnectionFactory(string.Empty));
    }
}
