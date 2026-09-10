using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace DevTaskManager.Api.Tests;

public class TasksApiTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TasksApiTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> LoginAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { username = "admin", password = "123456" });
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("token").GetString()!;
    }

    private void SetAuth(string token) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    private async Task<int> CreateTaskAsync(string title)
    {
        var response = await _client.PostAsJsonAsync("/api/tasks", new { title });
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("id").GetInt32();
    }

    // Teste 1: criar task deve retornar 201 e a task deve aparecer na listagem
    [Fact]
    public async Task CreateTask_ShouldReturnCreated_AndTaskShouldAppearInList()
    {
        SetAuth(await LoginAsync());

        var id = await CreateTaskAsync("Minha primeira task");

        var listResponse = await _client.GetAsync("/api/tasks");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var tasks = await listResponse.Content.ReadFromJsonAsync<List<TaskDto>>();
        Assert.Contains(tasks!, t => t.Id == id && t.Title == "Minha primeira task" && !t.IsCompleted);
    }

    // Teste 2: excluir task deve retornar 204 e um GET posterior deve retornar 404
    [Fact]
    public async Task DeleteTask_ShouldReturnNoContent_AndSubsequentGetShouldReturnNotFound()
    {
        SetAuth(await LoginAsync());

        var id = await CreateTaskAsync("Task para deletar");

        var deleteResponse = await _client.DeleteAsync($"/api/tasks/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/tasks/{id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    // Teste 3: acessar tasks sem token deve retornar 401 (JWT obrigatório)
    [Fact]
    public async Task GetTasks_WithoutToken_ShouldReturnUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private record TaskDto(int Id, string Title, bool IsCompleted, DateTime CreatedAt);
}
