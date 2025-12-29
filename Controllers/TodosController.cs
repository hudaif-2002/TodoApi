using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly TodoDbContext _context;
    private readonly ILogger<TodosController> _logger;

    public TodosController(TodoDbContext context, ILogger<TodosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodos()
    {
        _logger.LogInformation("Getting all todos");
        return await _context.TodoItems.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItem>> GetTodo(int id)
    {
        _logger.LogInformation("Getting todo with id {Id}", id);
        var todo = await _context.TodoItems.FindAsync(id);
        if (todo == null)
        {
            return NotFound(new { message = $"Todo with id {id} not found" });
        }
        return todo;
    }

    [HttpPost]
    public async Task<ActionResult<TodoItem>> CreateTodo(TodoItem todo)
    {
        _logger.LogInformation("Creating new todo: {Title}", todo.Title);
        todo.CreatedAt = DateTime.UtcNow;
        _context.TodoItems.Add(todo);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, todo);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTodo(int id, TodoItem todo)
    {
        if (id != todo.Id)
        {
            return BadRequest(new { message = "ID mismatch" });
        }
        var existingTodo = await _context.TodoItems.FindAsync(id);
        if (existingTodo == null)
        {
            return NotFound(new { message = $"Todo with id {id} not found" });
        }
        existingTodo.Title = todo.Title;
        existingTodo.Description = todo.Description;
        existingTodo.IsCompleted = todo.IsCompleted;
        if (todo.IsCompleted && existingTodo.CompletedAt == null)
        {
            existingTodo.CompletedAt = DateTime.UtcNow;
        }
        else if (!todo.IsCompleted)
        {
            existingTodo.CompletedAt = null;
        }
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodo(int id)
    {
        var todo = await _context.TodoItems.FindAsync(id);
        if (todo == null)
        {
            return NotFound(new { message = $"Todo with id {id} not found" });
        }
        _context.TodoItems.Remove(todo);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}