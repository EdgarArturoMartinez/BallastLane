using Ballastlane.Domain.Entities;

namespace Ballastlane.Application.Ports;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task CreateAsync(User user, CancellationToken ct = default);
}
