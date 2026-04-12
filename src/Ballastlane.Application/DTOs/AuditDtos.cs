using System.Text.Json;

namespace Ballastlane.Application.DTOs;

public record AuditDto(
    Guid Id,
    string Entity,
    string EntityId,
    string Action,
    Guid? UserId,
    string? Username,
    object? OldValues,
    object? NewValues,
    DateTime CreatedAt
);

public static class AuditDtoMapper
{
    public static AuditDto FromReader(Guid id, string entity, string entityId, string action, Guid? userId, string? username, string? oldJson, string? newJson, DateTime createdAt)
    {
        object? oldObj = null;
        object? newObj = null;
        try { if (!string.IsNullOrWhiteSpace(oldJson)) oldObj = JsonSerializer.Deserialize<object>(oldJson); } catch {}
        try { if (!string.IsNullOrWhiteSpace(newJson)) newObj = JsonSerializer.Deserialize<object>(newJson); } catch {}
        // Ensure CreatedAt is treated as UTC when serialized to JSON in the API
        var createdAtUtc = DateTime.SpecifyKind(createdAt, DateTimeKind.Utc);
        return new AuditDto(id, entity, entityId, action, userId, username, oldObj, newObj, createdAtUtc);
    }
}
