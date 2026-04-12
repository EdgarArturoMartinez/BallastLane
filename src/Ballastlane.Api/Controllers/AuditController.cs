using Ballastlane.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ballastlane.Application.DTOs;
using System.Globalization;

namespace Ballastlane.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuditDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? entity = null, [FromQuery] string? action = null, [FromQuery] string? q = null, CancellationToken ct = default)
    {
        var result = await _auditService.GetPagedAsync(page, pageSize, entity, action, q, ct);
        return Ok(result);
    }

    [HttpGet("meta")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Meta(CancellationToken ct)
    {
        var (entities, actions) = await _auditService.GetMetadataAsync(ct);

        // Server-side timezone and culture info to help clients show both server and local times
        var serverNowUtc = DateTime.UtcNow;
        var localTz = TimeZoneInfo.Local;
        var tzId = localTz.Id;
        var offset = localTz.GetUtcOffset(DateTime.UtcNow);
        var culture = CultureInfo.CurrentCulture.Name; // e.g. en-US

        // Format offset as +HH:mm or -HH:mm
        var sign = offset < TimeSpan.Zero ? "-" : "+";
        var offsetFmt = string.Format("{0}{1:D2}:{2:D2}", sign, Math.Abs(offset.Hours), Math.Abs(offset.Minutes));

        // Server local time (ISO) — helpful when the client cannot map Windows timezones
        var serverLocalNow = TimeZoneInfo.ConvertTimeFromUtc(serverNowUtc, localTz).ToString("o");

        return Ok(new
        {
            entities = entities.ToArray(),
            actions = actions.ToArray(),
            server = new
            {
                now = serverNowUtc.ToString("o"),
                localNow = serverLocalNow,
                timezone = tzId,
                offset = offsetFmt,
                culture = culture
            }
        });
    }
}
