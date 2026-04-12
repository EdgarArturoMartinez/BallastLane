namespace Ballastlane.Application.DTOs;

public sealed record PagedResult<T>(IEnumerable<T> Items, int TotalCount);
