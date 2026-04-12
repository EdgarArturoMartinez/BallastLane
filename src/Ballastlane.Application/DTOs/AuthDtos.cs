namespace Ballastlane.Application.DTOs;

// ── Auth DTOs ──────────────────────────────────────────────────────────────

public record RegisterRequest(
    string Username,
    string Email,
    string Password);

public record LoginRequest(
    string Username,
    string Password);

public record LoginResponse(
    string Token,
    string Username,
    string Role);

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    string Role);
