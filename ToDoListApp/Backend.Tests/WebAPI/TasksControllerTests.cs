using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ToDo.Application.Tasks;
using TaskStatus = ToDo.Domain.Enums.TaskStatus;

namespace Todo.Backend.Tests.WebAPI;

public class TasksControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client;

    public TasksControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TaskCrudEndpoints_WorkThroughHttpApi()
    {
        var createdId = await CreateTaskAsync("Build API", "Prepare frontend contract");

        var createdTask = await _client.GetFromJsonAsync<TaskDto>($"/api/tasks/{createdId}", JsonOptions);
        Assert.NotNull(createdTask);
        Assert.Equal(createdId, createdTask.Id);
        Assert.Equal("Build API", createdTask.Title);
        Assert.Equal(TaskStatus.Todo, createdTask.Status);

        var list = await _client.GetFromJsonAsync<IReadOnlyCollection<TaskDto>>("/api/tasks", JsonOptions);
        Assert.NotNull(list);
        Assert.Contains(list, task => task.Id == createdId);

        var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{createdId}", new
        {
            title = "Connect frontend",
            description = "Use API contract",
            deadline = new DateTime(2026, 5, 22, 12, 0, 0, DateTimeKind.Utc)
        });
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var statusResponse = await _client.PatchAsJsonAsync($"/api/tasks/{createdId}/status", new
        {
            status = "InProgress"
        });
        Assert.Equal(HttpStatusCode.NoContent, statusResponse.StatusCode);

        var changedTask = await _client.GetFromJsonAsync<TaskDto>($"/api/tasks/{createdId}", JsonOptions);
        Assert.NotNull(changedTask);
        Assert.Equal("Connect frontend", changedTask.Title);
        Assert.Equal(TaskStatus.InProgress, changedTask.Status);

        var deleteResponse = await _client.DeleteAsync($"/api/tasks/{createdId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var missingResponse = await _client.GetAsync($"/api/tasks/{createdId}");
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
    }

    [Fact]
    public async Task CreateTask_WithInvalidTitle_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/tasks", new
        {
            title = " ",
            description = "Invalid task",
            deadline = (DateTime?)null
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        using var problem = JsonDocument.Parse(content);
        Assert.True(
            TryGetProperty(problem.RootElement, "title", out var title),
            $"Expected validation problem title. Response: {content}");
        Assert.Equal("Validation Error", title.GetString());
        Assert.True(
            TryGetProperty(problem.RootElement, "errors", out var errors) && errors.TryGetProperty("title", out _),
            $"Expected title validation error. Response: {content}");
    }

    [Fact]
    public async Task ChangeTaskStatus_WithInvalidStatus_ReturnsBadRequest()
    {
        var createdId = await CreateTaskAsync("Validate status", null);

        var response = await _client.PatchAsJsonAsync($"/api/tasks/{createdId}/status", new
        {
            status = "Blocked"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MissingTask_ReturnsNotFoundProblem()
    {
        var response = await _client.GetAsync($"/api/tasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<HttpProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("Not Found", problem.Title);
    }

    [Fact]
    public async Task CorsPreflight_FromConfiguredFrontendOrigin_IsAllowed()
    {
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/tasks");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var origins));
        Assert.Contains("http://localhost:5173", origins);
    }

    private async Task<Guid> CreateTaskAsync(string title, string? description)
    {
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", new
        {
            title,
            description,
            deadline = new DateTime(2026, 5, 20, 12, 0, 0, DateTimeKind.Utc)
        });

        var content = await createResponse.Content.ReadAsStringAsync();
        Assert.True(
            createResponse.StatusCode == HttpStatusCode.Created,
            $"Expected Created but got {createResponse.StatusCode}. Response: {content}");

        return await createResponse.Content.ReadFromJsonAsync<Guid>();
    }

    private sealed record HttpProblemDetails(string? Title, string? Detail, int? Status);

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.TryGetProperty(propertyName, out value))
        {
            return true;
        }

        var pascalCaseName = char.ToUpperInvariant(propertyName[0]) + propertyName[1..];
        return element.TryGetProperty(pascalCaseName, out value);
    }
}
