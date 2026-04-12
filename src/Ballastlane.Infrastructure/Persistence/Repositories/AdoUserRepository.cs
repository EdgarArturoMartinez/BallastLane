using Ballastlane.Application.Ports;
using Ballastlane.Domain.Entities;
using Ballastlane.Domain.ValueObjects;
using Microsoft.Data.SqlClient;
using Ballastlane.Infrastructure.SqlQueries;

namespace Ballastlane.Infrastructure.Persistence.Repositories;

/// <summary>
/// ADO.NET adapter implementing IUserRepository.
/// Parameterized queries throughout — OWASP SQL-Injection safe.
/// </summary>
public sealed class AdoUserRepository : IUserRepository
{
    private readonly SqlConnectionFactory _factory;

    public AdoUserRepository(SqlConnectionFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var sql = SqlQueryLoader.Get("Users.GetById");

        await using var conn = await _factory.CreateOpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? MapUser(reader) : null;
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        var sql = SqlQueryLoader.Get("Users.GetByUsername");

        await using var conn = await _factory.CreateOpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@username", username);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? MapUser(reader) : null;
    }

    public async Task CreateAsync(User user, CancellationToken ct = default)
    {
        var sql = SqlQueryLoader.Get("Users.Create");

        await using var conn = await _factory.CreateOpenConnectionAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id",           user.Id);
        cmd.Parameters.AddWithValue("@username",     user.Username);
        cmd.Parameters.AddWithValue("@email",        user.Email.Value);
        cmd.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@salt",         user.Salt);
        cmd.Parameters.AddWithValue("@role",         user.Role);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    // ── Mapping ────────────────────────────────────────────────────────────

    private static User MapUser(SqlDataReader r) =>
        new(
            id:           r.GetGuid(0),
            username:     r.GetString(1),
            email:        Email.Create(r.GetString(2)),
            passwordHash: r.GetString(3),
            salt:         r.GetString(4),
            role:         r.GetString(5));
}
