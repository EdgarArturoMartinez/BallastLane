using Ballastlane.Application.DTOs;

namespace Ballastlane.Application.Ports;

public interface IAuditRepository
{
    Task<IEnumerable<AuditDto>> ListAsync(CancellationToken ct = default);
    Task<PagedResult<AuditDto>> ListPagedAsync(int page, int pageSize, string? entity = null, string? action = null, string? query = null, CancellationToken ct = default);
    Task<IEnumerable<string>> ListDistinctEntitiesAsync(CancellationToken ct = default);
    Task<IEnumerable<string>> ListDistinctActionsAsync(CancellationToken ct = default);
    Task InsertAsync(
        string entity,
        string entityId,
        string action,
        Guid? userId,
        string? username,
        object? oldValues,
        object? newValues,
        CancellationToken ct = default);
}
