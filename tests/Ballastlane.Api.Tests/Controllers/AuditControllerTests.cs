using System.Net.Http.Headers;
using System.Text.Json;
using Ballastlane.Api.Tests.Infrastructure;
using Ballastlane.Application.DTOs;

namespace Ballastlane.Api.Tests.Controllers;

/// <summary>
/// Integration tests for AuditController (GET /api/audit and GET /api/audit/meta).
///
/// Tests run against the real ASP.NET pipeline with in-memory fakes.
/// Each test that calls a protected endpoint first obtains a JWT via register+login.
/// </summary>
[Collection("Api")]
public sealed class AuditControllerTests
{
    private readonly HttpClient _client;
    private readonly BallastlaneWebApplicationFactory _factory;

    public AuditControllerTests(BallastlaneWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private record RegisterRequest(string Username, string Email, string Password);
    private record LoginRequest(string Username, string Password);
    private record LoginResponse(string Token, string Username, string Role);

    /// <summary>Registers a fresh user and returns their JWT.</summary>
    private async Task<string> CreateUserAndGetTokenAsync(string username)
    {
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(username, $"{username}@example.com", "P@ssword123!"));

        var resp = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(username, "P@ssword123!"));

        var body = await resp.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.Token;
    }

    private HttpClient AuthorizedClient(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    // ── Authentication guard ─────────────────────────────────────────────

    [Fact]
    public async Task GetAll_Returns_401_Without_Token()
    {
        var response = await _client.GetAsync("/api/audit");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Meta_Returns_401_Without_Token()
    {
        var response = await _client.GetAsync("/api/audit/meta");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── GET /api/audit ───────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_Returns_200_With_PagedResult_For_Authenticated_User()
    {
        _factory.Users.Clear();
        _factory.Audits.Clear();

        var token = await CreateUserAndGetTokenAsync("audituser1");
        var authorizedClient = AuthorizedClient(token);

        var response = await authorizedClient.GetAsync("/api/audit");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("items", out _), "Response should contain 'items' property");
        Assert.True(doc.RootElement.TryGetProperty("totalCount", out _), "Response should contain 'totalCount' property");
    }

    [Fact]
    public async Task GetAll_Returns_AuditRecords_Created_During_Registration()
    {
        _factory.Users.Clear();
        _factory.Audits.Clear();

        var token = await CreateUserAndGetTokenAsync("audituser2");
        var authorizedClient = AuthorizedClient(token);

        var response = await authorizedClient.GetAsync("/api/audit?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        // Registration and login each insert one audit record
        var totalCount = doc.RootElement.GetProperty("totalCount").GetInt32();
        Assert.True(totalCount >= 1, "Should have at least one audit row after register+login");
    }

    [Fact]
    public async Task GetAll_Supports_Pagination_Parameters()
    {
        _factory.Users.Clear();
        _factory.Audits.Clear();

        var token = await CreateUserAndGetTokenAsync("audituser3");
        var authorizedClient = AuthorizedClient(token);

        var response = await authorizedClient.GetAsync("/api/audit?page=1&pageSize=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var items = doc.RootElement.GetProperty("items");
        Assert.True(items.GetArrayLength() <= 1, "Page size of 1 should return at most 1 item");
    }

    // ── GET /api/audit/meta ──────────────────────────────────────────────

    [Fact]
    public async Task Meta_Returns_200_With_Entities_Actions_And_Server_Info()
    {
        _factory.Users.Clear();
        _factory.Audits.Clear();

        var token = await CreateUserAndGetTokenAsync("audituser4");
        var authorizedClient = AuthorizedClient(token);

        var response = await authorizedClient.GetAsync("/api/audit/meta");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("entities", out _), "Should contain 'entities'");
        Assert.True(doc.RootElement.TryGetProperty("actions", out _), "Should contain 'actions'");
        Assert.True(doc.RootElement.TryGetProperty("server", out var server), "Should contain 'server'");
        Assert.True(server.TryGetProperty("now", out _), "server should contain 'now'");
        Assert.True(server.TryGetProperty("timezone", out _), "server should contain 'timezone'");
    }

    [Fact]
    public async Task Meta_Entities_And_Actions_Reflect_Audit_Inserts()
    {
        _factory.Users.Clear();
        _factory.Audits.Clear();

        var token = await CreateUserAndGetTokenAsync("audituser5");
        var authorizedClient = AuthorizedClient(token);

        var response = await authorizedClient.GetAsync("/api/audit/meta");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var entities = doc.RootElement.GetProperty("entities").EnumerateArray()
                          .Select(e => e.GetString()).ToList();
        var actions = doc.RootElement.GetProperty("actions").EnumerateArray()
                         .Select(a => a.GetString()).ToList();

        // Registration creates a "Users"/"Created" record and login creates "Users"/"LoggedIn"
        Assert.Contains("Users", entities);
        Assert.True(actions.Count >= 1, "Should have at least one distinct action");
    }
}
