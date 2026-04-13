using Ballastlane.Infrastructure.SqlQueries;

namespace Ballastlane.Infrastructure.Tests.Persistence;

/// <summary>
/// Unit tests for SqlQueryLoader.
/// Verifies that all known SQL files are discoverable and have content,
/// and that requesting a non-existent query name throws FileNotFoundException.
/// </summary>
public sealed class SqlQueryLoaderTests
{
    // ── Known queries ────────────────────────────────────────────────────

    [Theory]
    [InlineData("Users.GetById")]
    [InlineData("Users.GetByUsername")]
    [InlineData("Users.Create")]
    [InlineData("Tasks.List")]
    [InlineData("Tasks.GetById")]
    [InlineData("Tasks.Create")]
    [InlineData("Tasks.Update")]
    [InlineData("Tasks.Delete")]
    [InlineData("Audits.List")]
    [InlineData("Audits.Insert")]
    [InlineData("Audits.DistinctEntities")]
    [InlineData("Audits.DistinctActions")]
    public void Get_KnownQuery_ReturnsNonEmptyContent(string name)
    {
        var sql = SqlQueryLoader.Get(name);

        Assert.False(string.IsNullOrWhiteSpace(sql));
    }

    // ── Missing queries ──────────────────────────────────────────────────

    [Fact]
    public void Get_UnknownQuery_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(
            () => SqlQueryLoader.Get("DoesNotExist.Query"));
    }

    [Fact]
    public void Get_EmptyName_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(
            () => SqlQueryLoader.Get(""));
    }
}
