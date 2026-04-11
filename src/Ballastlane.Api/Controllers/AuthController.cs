using Ballastlane.Application.DTOs;
using Ballastlane.Application.Services;
using Ballastlane.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ballastlane.Api.Controllers;

/// <summary>
/// Authentication endpoints — all public (no [Authorize]).
///
/// SOLID: SRP — HTTP mapping only; all credential logic lives in IAuthService.
/// Security: returns generic error messages to prevent user enumeration (OWASP A07).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Registers a new user account.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var dto = await _authService.RegisterAsync(request, ct);
        return CreatedAtAction(nameof(Register), new { id = dto.Id }, dto);
    }

    /// <summary>Authenticates a user and returns a JWT token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        try
        {
            var response = await _authService.LoginAsync(request, ct);
            return Ok(response);
        }
        catch (DomainException)
        {
            // Return generic 401 — never reveal whether username or password was wrong (OWASP A07)
            return Unauthorized(new { error = "Invalid username or password." });
        }
    }

    /// <summary>Public endpoint — used to smoke-test that the API is reachable.</summary>
    [HttpGet("ping")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Ping() => Ok(new { message = "pong", utc = DateTime.UtcNow });
}
