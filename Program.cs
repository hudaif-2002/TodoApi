using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Database - CHANGED TO POSTGRESQL
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Create database and apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    db.Database.EnsureCreated();
}

// Configure middleware
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// Simple root endpoint
app.MapGet("/", () => new 
{
    message = "TodoApi is running with PostgreSQL!",
    database = "Supabase PostgreSQL",
    endpoints = new 
    {
        getAllTodos = "/api/todos",
        getTodo = "/api/todos/{id}",
        createTodo = "POST /api/todos",
        updateTodo = "PUT /api/todos/{id}",
        deleteTodo = "DELETE /api/todos/{id}",
        swagger = "/swagger"
    }
});

// Listen on PORT environment variable (for Railway/Render)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();