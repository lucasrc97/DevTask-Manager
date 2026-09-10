using DevTaskManager.Api.Data;
using DevTaskManager.Api.Dtos;
using DevTaskManager.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevTaskManager.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _context;

    public TasksController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/tasks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskResponse>>> GetAll()
    {
        var tasks = await _context.Tasks
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TaskResponse(t.Id, t.Title, t.IsCompleted, t.CreatedAt))
            .ToListAsync();

        return Ok(tasks);
    }

    // GET /api/tasks/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskResponse>> GetById(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task is null)
            return NotFound(new { message = "Task não encontrada." });

        return Ok(new TaskResponse(task.Id, task.Title, task.IsCompleted, task.CreatedAt));
    }

    // POST /api/tasks
    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create([FromBody] CreateTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "O título é obrigatório." });

        var task = new TaskItem { Title = request.Title.Trim() };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = task.Id },
            new TaskResponse(task.Id, task.Title, task.IsCompleted, task.CreatedAt));
    }

    // PUT /api/tasks/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "O título é obrigatório." });

        var task = await _context.Tasks.FindAsync(id);
        if (task is null)
            return NotFound(new { message = "Task não encontrada." });

        task.Title = request.Title.Trim();
        task.IsCompleted = request.IsCompleted;
        await _context.SaveChangesAsync();

        return Ok(new TaskResponse(task.Id, task.Title, task.IsCompleted, task.CreatedAt));
    }

    // DELETE /api/tasks/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task is null)
            return NotFound(new { message = "Task não encontrada." });

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
