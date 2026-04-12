using Ballastlane.Application.DTOs;
using Ballastlane.Application.Ports;
using System.Text.Json;

namespace Ballastlane.Api.Tests.Fakes;

/// <summary>
/// In-memory IAuditRepository test double — no SQL Server required.
/// </summary>
public sealed class FakeAuditRepository : IAuditRepository
{
    private readonly List<AuditDto> _records = new();

    public Task InsertAsync(
        string entity, string entityId, string action,
        Guid? userId, string? username,
        object? oldValues, object? newValues,
        CancellationToken ct = default)
    {
        _records.Add(new AuditDto(
            Guid.NewGuid(), entity, entityId, action,
            userId, username, oldValues, newValues,
            DateTime.UtcNow));
        return Task.CompletedTask;
    }

    public Task<IEnumerable<AuditDto>> ListAsync(CancellationToken ct = default) =>
        Task.FromResult<IEnumerable<AuditDto>>(_records.ToList());

    public Task<PagedResult<AuditDto>> ListPagedAsync(
        int page, int pageSize,
        string? entity = null, string? action = null, string? query = null,
        CancellationToken ct = default)
    {
        var filtered = _records.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(entity))
            filtered = filtered.Where(r => r.Entity.Equals(entity, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(action))
            filtered = filtered.Where(r => r.Action.Equals(action, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(query))
            filtered = filtered.Where(r =>
                r.Entity.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                r.Action.Contains(query, StringComparison.OrdinalIgnoreCase));

        var list = filtered.ToList();
        var items = list.Skip((page - 1) * pageSize).Take(pageSize);
        return Task.FromResult(new PagedResult<AuditDto>(items, list.Count));
    }

    public Task<IEnumerable<string>> ListDistinctEntitiesAsync(CancellationToken ct = default) =>
        Task.FromResult(_records.Select(r => r.Entity).Distinct());

    public Task<IEnumerable<string>> ListDistinctActionsAsync(CancellationToken ct = default) =>
        Task.FromResult(_records.Select(r => r.Action).Distinct());

    public void Clear() => _records.Clear();
}
