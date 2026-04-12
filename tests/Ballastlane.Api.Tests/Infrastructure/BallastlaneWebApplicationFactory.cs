using Ballastlane.Api.Tests.Fakes;
using Ballastlane.Application.Ports;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ballastlane.Api.Tests.Infrastructure;

/// <summary>
/// Shared WebApplicationFactory for all API integration tests.
///
/// Registered as a collection fixture (see ApiTestCollection) so only ONE
/// factory is ever started per test run.  This avoids the Serilog
/// "logger is already frozen" error that occurs when multiple factories
/// try to initialise the static Log.Logger concurrently.
///
/// Strategy:
///   - "Testing" environment → Program.cs skips the DB migration block
///     and the Serilog file-sink (preventing log-file race conditions).
///   - appsettings.json JWT config is used unchanged — both JwtTokenGenerator
///     and the JWT Bearer middleware read from the same source, so tokens
///     generated in tests are valid when validated.
///   - Only ITaskRepository and IUserRepository are swapped for in-memory fakes.
/// </summary>
public sealed class BallastlaneWebApplicationFactory : WebApplicationFactory<Program>
{
    // Expose fakes so tests can pre-seed or inspect state
    public FakeTaskRepository Tasks { get; } = new();
    public FakeUserRepository Users { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Signal Program.cs to skip DB migration/seed AND Serilog file-sink
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the ADO.NET repository implementations registered by AddInfrastructure
            services.RemoveAll<ITaskRepository>();
            services.RemoveAll<IUserRepository>();

            // Register in-memory fakes (singleton so state persists across requests in one test)
            // Safe: TaskService is Scoped and a scoped service consuming a singleton is legal in DI
            services.AddSingleton<ITaskRepository>(Tasks);
            services.AddSingleton<IUserRepository>(Users);
        });
    }
}

/// <summary>
/// xUnit collection definition — causes both AuthControllerTests and
/// TasksControllerTests to share the SAME BallastlaneWebApplicationFactory
/// instance, starting the test server only once.
/// </summary>
[CollectionDefinition("Api")]
public class ApiTestCollection : ICollectionFixture<BallastlaneWebApplicationFactory> { }
