using Ballastlane.Application.Ports;
using Ballastlane.Domain.Entities;
using Ballastlane.Domain.ValueObjects;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Ballastlane.Infrastructure.Persistence;

/// <summary>
/// Ensures the demo user's password hash and salt are properly computed on first startup.
/// The SQL seed script inserts placeholder values; this seeder replaces them using the
/// real PBKDF2 hasher so we never store plain-text passwords anywhere.
///
/// Pattern: Init Once (idempotent bootstrap).
/// SOLID: SRP — only responsible for seeding the demo account credentials.
/// </summary>
public sealed class DbSeeder
{
    private readonly SqlConnectionFactory _factory;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<DbSeeder> _logger;

    private const string DemoUsername = "demo";
    private const string DemoPassword = "Demo@12345";
    private const string PlaceholderMark = "SEED_HASH_REPLACE_ON_FIRST_RUN";

    public DbSeeder(SqlConnectionFactory factory, IPasswordHasher hasher, ILogger<DbSeeder> logger)
    {
        _factory = factory;
        _hasher  = hasher;
        _logger  = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await using var conn = await _factory.CreateOpenConnectionAsync(ct);

        // Check if the demo user's hash is still the placeholder
        const string check = """
            SELECT PasswordHash FROM Users WHERE Username = @username
            """;

        await using var checkCmd = new SqlCommand(check, conn);
        checkCmd.Parameters.AddWithValue("@username", DemoUsername);

        var existing = await checkCmd.ExecuteScalarAsync(ct) as string;

        if (existing is null)
        {
            _logger.LogInformation("Demo user not found — skipping seed (migration may not have run).");
            return;
        }

        if (!existing.StartsWith("SEED_", StringComparison.Ordinal))
        {
            _logger.LogInformation("Demo user already seeded — no action needed.");
            return;
        }

        var salt = _hasher.GenerateSalt();
        var hash = _hasher.Hash(DemoPassword, salt);

        const string update = """
            UPDATE Users SET PasswordHash = @hash, Salt = @salt WHERE Username = @username
            """;

        await using var updateCmd = new SqlCommand(update, conn);
        updateCmd.Parameters.AddWithValue("@hash",     hash);
        updateCmd.Parameters.AddWithValue("@salt",     salt);
        updateCmd.Parameters.AddWithValue("@username", DemoUsername);
        await updateCmd.ExecuteNonQueryAsync(ct);

        _logger.LogInformation("Demo user credentials seeded successfully.");
    }
}
