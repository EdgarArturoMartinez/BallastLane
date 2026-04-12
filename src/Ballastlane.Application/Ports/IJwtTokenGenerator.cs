using Ballastlane.Domain.Entities;

namespace Ballastlane.Application.Ports;

/// <summary>
/// Port: JWT token generation abstraction.
/// SOLID: DIP — keeps auth business logic independent of jwt library details.
/// Pattern: Strategy — swap to different signing algorithms (RS256, ES256) without changing AuthService.
/// </summary>
public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
