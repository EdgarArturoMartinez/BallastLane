using Ballastlane.Application.DTOs;
using Ballastlane.Application.Ports;

namespace Ballastlane.Application.Services;

public sealed class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepository;

    public AuditService(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository ?? throw new ArgumentNullException(nameof(auditRepository));
    }

    public async Task<IEnumerable<AuditDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _auditRepository.ListAsync(ct);
    }

    public async Task<PagedResult<AuditDto>> GetPagedAsync(int page, int pageSize, string? entity = null, string? action = null, string? query = null, CancellationToken ct = default)
    {
        return await _auditRepository.ListPagedAsync(page, pageSize, entity, action, query, ct);
    }

    public async Task<(IEnumerable<string> Entities, IEnumerable<string> Actions)> GetMetadataAsync(CancellationToken ct = default)
    {
        var entities = await _auditRepository.ListDistinctEntitiesAsync(ct);
        var actions = await _auditRepository.ListDistinctActionsAsync(ct);
        return (entities, actions);
    }
}
