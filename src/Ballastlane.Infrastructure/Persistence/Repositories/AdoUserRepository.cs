using Ballastlane.Application.Ports;
using Ballastlane.Domain.Entities;
using Ballastlane.Domain.ValueObjects;
using Microsoft.Data.SqlClient;
using Ballastlane.Infrastructure.SqlQueries;

namespace Ballastlane.Infrastructure.Persistence.Repositories;

/// <summary>
/// ADO.NET adapter implementing IUserRepository.
/// SQL text is loaded from SqlQueries/*.sql (centralized, versionable).
/// Execution is delegated to DbExecutor — no ADO.NET boilerplate here.
/// </summary>
public sealed class AdoUserRepository : IUserRepository
{
    private readonly DbExecutor _db;

    public AdoUserRepository(DbExecutor db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.QuerySingleOrDefaultAsync(
            SqlQueryLoader.Get("Users.GetById"),
            cmd => cmd.Parameters.AddWithValue("@id", id),
            MapUser,
            ct);

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        _db.QuerySingleOrDefaultAsync(
            SqlQueryLoader.Get("Users.GetByUsername"),
            cmd => cmd.Parameters.AddWithValue("@username", username),
            MapUser,
            ct);

    public Task CreateAsync(User user, CancellationToken ct = default) =>
        _db.ExecuteAsync(
            SqlQueryLoader.Get("Users.Create"),
            cmd =>
            {
                cmd.Parameters.AddWithValue("@id",           user.Id);
                cmd.Parameters.AddWithValue("@username",     user.Username);
                cmd.Parameters.AddWithValue("@email",        user.Email.Value);
                cmd.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
                cmd.Parameters.AddWithValue("@salt",         user.Salt);
                cmd.Parameters.AddWithValue("@role",         user.Role);
            },
            ct);

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
