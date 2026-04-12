using Ballastlane.Application.DTOs;
using Ballastlane.Application.Ports;
using Ballastlane.Domain.Entities;
using Ballastlane.Domain.Exceptions;
using Ballastlane.Domain.ValueObjects;

namespace Ballastlane.Application.Services;

/// <summary>
/// Application service — implements user registration and login use-cases.
///
/// SOLID:
///   SRP — handles only authentication orchestration; password hashing and token
///         generation are delegated to dedicated ports (DIP).
///   OCP — swap IPasswordHasher or IJwtTokenGenerator without touching this class.
///
/// Pattern: Strategy (via IPasswordHasher), Service Layer.
/// Security: passwords never stored in plain text; timing-safe comparison via IPasswordHasher.Verify.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtGenerator;
    private readonly IAuditRepository _auditRepository;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtGenerator,
        IAuditRepository auditRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _jwtGenerator = jwtGenerator ?? throw new ArgumentNullException(nameof(jwtGenerator));
        _auditRepository = auditRepository ?? throw new ArgumentNullException(nameof(auditRepository));
    }

    public async Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new DomainException("Password must be at least 8 characters.");

        var existing = await _userRepository.GetByUsernameAsync(request.Username, ct);
        if (existing is not null)
            throw new DomainException($"Username '{request.Username}' is already taken.");

        var email = Email.Create(request.Email);
        var salt = _passwordHasher.GenerateSalt();
        var hash = _passwordHasher.Hash(request.Password, salt);

        var user = new User(Guid.NewGuid(), request.Username, email, hash, salt);
        await _userRepository.CreateAsync(user, ct);

        // Audit: record user creation (do NOT include sensitive fields)
        try
        {
            var newValues = new { user.Id, user.Username, Email = user.Email.Value, user.Role };
            if (_auditRepository is not null)
                await _auditRepository.InsertAsync("Users", user.Id.ToString(), "Created", user.Id, user.Username, null, newValues, ct);
        }
        catch
        {
            // Audit failures must not block registration flow; swallow safely
        }

        return new UserDto(user.Id, user.Username, user.Email.Value, user.Role);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _userRepository.GetByUsernameAsync(request.Username, ct)
            ?? throw new DomainException("Invalid username or password.");

        if (!_passwordHasher.Verify(request.Password, user.Salt, user.PasswordHash))
            throw new DomainException("Invalid username or password.");

        var token = _jwtGenerator.GenerateToken(user);
        // Audit: record successful login (do NOT include sensitive fields)
        try
        {
            var newValues = new { user.Id, user.Username, user.Role };
            if (_auditRepository is not null)
                await _auditRepository.InsertAsync("Users", user.Id.ToString(), "LoggedIn", user.Id, user.Username, null, newValues, ct);
        }
        catch
        {
            // swallow — audit failures must not block authentication
        }

        return new LoginResponse(token, user.Username, user.Role);
    }
}
