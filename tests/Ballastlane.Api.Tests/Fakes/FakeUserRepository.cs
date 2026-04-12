using Ballastlane.Application.Ports;
using Ballastlane.Domain.Entities;

namespace Ballastlane.Api.Tests.Fakes;

/// <summary>
/// In-memory IUserRepository test double — no SQL Server required.
/// </summary>
public sealed class FakeUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        Task.FromResult(
            _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));

    public Task CreateAsync(User user, CancellationToken ct = default)
    {
        _users.Add(user);
        return Task.CompletedTask;
    }

    /// <summary>Resets the store between test cases that share a factory.</summary>
    public void Clear() => _users.Clear();
}
