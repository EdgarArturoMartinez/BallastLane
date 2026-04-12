using Ballastlane.Application.DTOs;

namespace Ballastlane.Application.Services;

public interface IAuditService
{
    Task<IEnumerable<AuditDto>> GetAllAsync(CancellationToken ct = default);
    Task<PagedResult<AuditDto>> GetPagedAsync(int page, int pageSize, string? entity = null, string? action = null, string? query = null, CancellationToken ct = default);
    Task<(IEnumerable<string> Entities, IEnumerable<string> Actions)> GetMetadataAsync(CancellationToken ct = default);
}
