using Ballastlane.Application.Ports;
using Ballastlane.Application.Services;
using Ballastlane.Application.DTOs;
using Ballastlane.Infrastructure.Persistence.Repositories;
using Ballastlane.Infrastructure.Persistence;
using Ballastlane.Infrastructure.Persistence.Repositories;
using Ballastlane.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ballastlane.Infrastructure;

/// <summary>
/// Extension method that wires all infrastructure services into the DI container.
///
/// Pattern: Extension Method (composition root helper).
/// SOLID:
///   SRP — registration details isolated here; API Program.cs stays clean.
///   DIP — callers depend on ITaskRepository / IUserRepository; never on concrete adapters.
/// </summary>
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Connection ─────────────────────────────────────────────────────
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Missing 'ConnectionStrings:DefaultConnection' in configuration.");

        services.AddSingleton(new SqlConnectionFactory(connectionString));

        // ── Repositories (secondary adapters) ─────────────────────────────
        services.AddScoped<ITaskRepository, AdoTaskRepository>();
        services.AddScoped<IUserRepository, AdoUserRepository>();
        services.AddScoped<IAuditRepository, AdoAuditRepository>();

        // ── Security (strategy adapters) ──────────────────────────────────
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()
            ?? throw new InvalidOperationException("Missing 'Jwt' section in configuration.");

        services.AddSingleton(jwtSettings);
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        // ── Application services ───────────────────────────────────────────
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuditService, AuditService>();

        // ── DB migration & seeder (registered for manual invocation at startup) ──
        services.AddSingleton(sp =>
            new DbMigrator(connectionString, sp.GetRequiredService<ILogger<DbMigrator>>()));

        services.AddSingleton<DbSeeder>();

        return services;
    }
}
