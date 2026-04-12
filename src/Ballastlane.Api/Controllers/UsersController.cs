using Ballastlane.Application.DTOs;
using Ballastlane.Application.Ports;
using Ballastlane.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ballastlane.Api.Controllers;

/// <summary>
/// User profile endpoints.
/// Demonstrates both [AllowAnonymous] (public) and [Authorize] (protected) on the same controller.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>Public endpoint — returns user summary by id (no sensitive data).</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user is null) return NotFound();

        return Ok(new UserDto(user.Id, user.Username, user.Email.Value, user.Role));
    }

    /// <summary>Authorized endpoint — returns the current caller's own profile.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(sub, out var userId))
            throw new DomainException("User identity could not be resolved.");

        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new DomainException("Authenticated user not found.");

        return Ok(new UserDto(user.Id, user.Username, user.Email.Value, user.Role));
    }
}
