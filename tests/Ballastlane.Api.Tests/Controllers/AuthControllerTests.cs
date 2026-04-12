using System.Net.Http.Headers;
using Ballastlane.Api.Tests.Infrastructure;
using Ballastlane.Application.DTOs;

namespace Ballastlane.Api.Tests.Controllers;

/// <summary>
/// Integration tests for AuthController.
///
/// Tests run against the real ASP.NET pipeline with in-memory repository fakes.
/// No SQL Server is required.
/// Uses the shared [Collection("Api")] factory to avoid Serilog re-initialisation errors.
/// </summary>
[Collection("Api")]
public sealed class AuthControllerTests
{
    private readonly HttpClient _client;
    private readonly BallastlaneWebApplicationFactory _factory;

    public AuthControllerTests(BallastlaneWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    // ── Helpers ─────────────────────────────────────────────────────────

    private static RegisterRequest ValidRegisterRequest(string username = "testuser") =>
        new(username, $"{username}@example.com", "P@ssword123!");

    // ── Register ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_Returns_201_With_User_Dto()
    {
        _factory.Users.Clear();

        var response = await _client.PostAsJsonAsync("/api/auth/register",
            ValidRegisterRequest("newuser"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(body);
        Assert.Equal("newuser", body.Username);
        Assert.False(string.IsNullOrWhiteSpace(body.Role));
    }

    [Fact]
    public async Task Register_Returns_400_On_Duplicate_Username()
    {
        _factory.Users.Clear();

        // First registration succeeds
        await _client.PostAsJsonAsync("/api/auth/register", ValidRegisterRequest("dupeuser"));

        // Second with same username must fail
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            ValidRegisterRequest("dupeuser"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_Returns_400_When_Password_Too_Short()
    {
        _factory.Users.Clear();

        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("shortpwduser", "shortpwduser@example.com", "abc"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── Login ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_Returns_200_With_Token_After_Successful_Register()
    {
        _factory.Users.Clear();

        await _client.PostAsJsonAsync("/api/auth/register", ValidRegisterRequest("logintest"));

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("logintest", "P@ssword123!"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));
        Assert.Equal("logintest", body.Username);
    }

    [Fact]
    public async Task Login_Returns_401_For_Wrong_Password()
    {
        _factory.Users.Clear();

        await _client.PostAsJsonAsync("/api/auth/register", ValidRegisterRequest("wrongpass"));

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("wrongpass", "W0ngPassword!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_Returns_401_For_Unknown_User()
    {
        _factory.Users.Clear();

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("nobody", "P@ssword123!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── Ping ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Ping_Returns_200_Without_Auth()
    {
        var response = await _client.GetAsync("/api/auth/ping");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── Private helpers ──────────────────────────────────────────────────

    /// <summary>Registers + logs in, returns the bearer token.</summary>
    private async Task<string> GetTokenAsync(string username = "authuser")
    {
        _factory.Users.Clear();
        await _client.PostAsJsonAsync("/api/auth/register", ValidRegisterRequest(username));

        var resp = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(username, "P@ssword123!"));

        var body = await resp.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.Token;
    }

    // ── Private DTO records matching the API contract ────────────────────
    private record RegisterRequest(string Username, string Email, string Password);
    private record LoginRequest(string Username, string Password);
    private record LoginResponse(string Token, string Username, string Role);
}
