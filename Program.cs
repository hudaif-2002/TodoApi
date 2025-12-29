using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Database - Convert Railway DATABASE_URL to Npgsql format
builder.Services.AddDbContext<TodoDbContext>(options =>
{
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
    
    if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("postgresql://"))
    {
        // Railway provides URI format, convert to Npgsql key-value format
        var uri = new Uri(connectionString);
        var npgsqlConnectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SSL Mode=Prefer;Trust Server Certificate=true";
        options.UseNpgsql(npgsqlConnectionString);
    }
    else
    {
        // Fallback to appsettings.json (Supabase for local testing)
        connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseNpgsql(connectionString);
    }
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
    database = "Railway PostgreSQL",
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
