using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Database - Use Railway DATABASE_URL or fallback to appsettings
builder.Services.AddDbContext<TodoDbContext>(options =>
{
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
        ?? builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

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

app.UseAuthorization();
app.MapControllers();

// Root endpoint
app.MapGet("/", () => new
{
    message = "TodoApi is running with PostgreSQL!",
    database = "Railway PostgreSQL (or Supabase fallback)",
    endpoints = new[]
    {
        "GET /api/todos - Get all todos",
        "GET /api/todos/{id} - Get todo by ID",
        "POST /api/todos - Create new todo",
        "PUT /api/todos/{id} - Update todo",
        "DELETE /api/todos/{id} - Delete todo"
    },
    swagger = "/swagger"
});

app.Run();
