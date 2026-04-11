using System.Net.Http.Headers;
using Ballastlane.Api.Tests.Infrastructure;
using Ballastlane.Application.DTOs;

namespace Ballastlane.Api.Tests.Controllers;

/// <summary>
/// Integration tests for TasksController.
///
/// Tests run against the real ASP.NET pipeline with in-memory repository fakes.
/// Each test obtains a valid JWT by calling the auth endpoint first.
/// Uses the shared [Collection("Api")] factory to avoid Serilog re-initialisation errors.
/// </summary>
[Collection("Api")]
public sealed class TasksControllerTests
{
    private readonly HttpClient _client;
    private readonly BallastlaneWebApplicationFactory _factory;

    public TasksControllerTests(BallastlaneWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private record RegisterRequest(string Username, string Email, string Password);
    private record LoginRequest(string Username, string Password);
    private record LoginResponse(string Token, string Username, string Role);

    private record CreateTaskPayload(
        string Title,
        string Description,
        DateTime? DueDate);

    private record UpdateTaskPayload(
        string Title,
        string Description,
        string Status,
        DateTime? DueDate);

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
        var response = await _client.GetAsync("/api/tasks");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns_401_Without_Token()
    {
        var response = await _client.PostAsJsonAsync("/api/tasks",
            new CreateTaskPayload("Title", "Desc", null));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── GetAll ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_Returns_200_And_Empty_List_For_New_User()
    {
        _factory.Tasks.Clear();
        _factory.Users.Clear();

        var token  = await CreateUserAndGetTokenAsync("listreader");
        var client = AuthorizedClient(token);

        var response = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tasks = await response.Content.ReadFromJsonAsync<TaskDto[]>();
        Assert.NotNull(tasks);
        Assert.Empty(tasks);
    }

    // ── Create ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_Returns_201_With_TaskDto()
    {
        _factory.Tasks.Clear();
        _factory.Users.Clear();

        var token  = await CreateUserAndGetTokenAsync("creator");
        var client = AuthorizedClient(token);

        var payload  = new CreateTaskPayload("My first task", "Some description", DateTime.UtcNow.AddDays(3));
        var response = await client.PostAsJsonAsync("/api/tasks", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<TaskDto>();
        Assert.NotNull(dto);
        Assert.Equal("My first task",    dto.Title);
        Assert.Equal("Todo",             dto.Status);
        Assert.NotEqual(Guid.Empty,      dto.Id);
    }

    [Fact]
    public async Task Create_Returns_400_When_Title_Is_Empty()
    {
        _factory.Tasks.Clear();
        _factory.Users.Clear();

        var token  = await CreateUserAndGetTokenAsync("badtitleuser");
        var client = AuthorizedClient(token);

        var response = await client.PostAsJsonAsync("/api/tasks",
            new CreateTaskPayload("", "desc", null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── GetById ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_Returns_200_For_Existing_Task()
    {
        _factory.Tasks.Clear();
        _factory.Users.Clear();

        var token  = await CreateUserAndGetTokenAsync("getbyiduser");
        var client = AuthorizedClient(token);

        // Create a task first, then fetch it
        var createResp = await client.PostAsJsonAsync("/api/tasks",
            new CreateTaskPayload("Fetchable task", "", null));
        var created = await createResp.Content.ReadFromJsonAsync<TaskDto>();

        var response = await client.GetAsync($"/api/tasks/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<TaskDto>();
        Assert.Equal(created.Id, dto!.Id);
    }

    [Fact]
    public async Task GetById_Returns_404_For_Unknown_Id()
    {
        _factory.Users.Clear();

        var token  = await CreateUserAndGetTokenAsync("notfounduser");
        var client = AuthorizedClient(token);

        var response = await client.GetAsync($"/api/tasks/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Update ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_Returns_200_With_Modified_Task()
    {
        _factory.Tasks.Clear();
        _factory.Users.Clear();

        var token  = await CreateUserAndGetTokenAsync("updater");
        var client = AuthorizedClient(token);

        var createResp = await client.PostAsJsonAsync("/api/tasks",
            new CreateTaskPayload("Old title", "Old desc", null));
        var created = await createResp.Content.ReadFromJsonAsync<TaskDto>();

        var updatePayload = new UpdateTaskPayload("New title", "New desc", "InProgress", null);
        var updateResp    = await client.PutAsJsonAsync($"/api/tasks/{created!.Id}", updatePayload);

        Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);
        var updated = await updateResp.Content.ReadFromJsonAsync<TaskDto>();
        Assert.Equal("New title",   updated!.Title);
        Assert.Equal("InProgress",  updated.Status);
    }

    // ── Delete ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_Returns_204_And_Task_No_Longer_Accessible()
    {
        _factory.Tasks.Clear();
        _factory.Users.Clear();

        var token  = await CreateUserAndGetTokenAsync("deleter");
        var client = AuthorizedClient(token);

        var createResp = await client.PostAsJsonAsync("/api/tasks",
            new CreateTaskPayload("Task to delete", "", null));
        var created = await createResp.Content.ReadFromJsonAsync<TaskDto>();

        var deleteResp = await client.DeleteAsync($"/api/tasks/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);

        // Subsequent GET should return 404
        var getResp = await client.GetAsync($"/api/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResp.StatusCode);
    }
}
