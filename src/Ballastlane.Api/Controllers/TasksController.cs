using Ballastlane.Application.DTOs;
using Ballastlane.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ballastlane.Api.Controllers;

/// <summary>
/// CRUD endpoints for Tasks. All routes require authentication.
///
/// SOLID: SRP — delegates all logic to ITaskService; controller only handles HTTP.
/// Pattern: Thin Controller (orchestration in Application layer, not here).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>Returns all tasks for the authenticated user's context.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tasks = await _taskService.GetAllAsync(ct);
        return Ok(tasks);
    }

    /// <summary>Returns a single task by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var task = await _taskService.GetByIdAsync(id, ct);
        return task is null ? NotFound() : Ok(task);
    }

    /// <summary>Creates a new task owned by the authenticated user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        var ownerId = GetCurrentUserId();
        var dto = await _taskService.CreateAsync(ownerId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    /// <summary>Updates title, description, status, and due date of an existing task.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken ct)
    {
        var dto = await _taskService.UpdateAsync(id, request, ct);
        return Ok(dto);
    }

    /// <summary>Deletes a task by id.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _taskService.DeleteAsync(id, ct);
        return NoContent();
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(sub, out var id)
            ? id
            : throw new UnauthorizedAccessException("User identity could not be resolved.");
    }
}
