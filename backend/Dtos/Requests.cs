namespace DevTaskManager.Api.Dtos;

public record LoginRequest(string Username, string Password);

public record CreateTaskRequest(string Title);

public record UpdateTaskRequest(string Title, bool IsCompleted);

public record TaskResponse(int Id, string Title, bool IsCompleted, DateTime CreatedAt);
