namespace Ballastlane.Infrastructure.Tests.Persistence;

/// <summary>
/// Binds the "Database" collection to DatabaseFixture.
/// All test classes decorated with [Collection("Database")] share one fixture instance,
/// meaning the SQL Server database and migrations run once per test run — not once per class.
/// </summary>
[CollectionDefinition("Database")]
public sealed class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }
