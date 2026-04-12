using Ballastlane.Application.DTOs;

namespace Ballastlane.Application.Services;

/// <summary>
/// Port (primary): Auth use-cases — register and login.
/// SOLID: ISP — separate from ITaskService; unrelated to task CRUD.
/// </summary>
public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
