using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SMART DATABASE CONFIGURATION - Works with ANY provider!
builder.Services.AddDbContext<TodoDbContext>(options =>
{
    var (provider, connectionString, source) = GetDatabaseConfig(builder.Configuration);
    
    if (provider == "SQLite")
    {
        options.UseSqlite(connectionString);
        Console.WriteLine($"Using SQLite database: {connectionString}");
    }
    else
    {
        options.UseNpgsql(connectionString);
        Console.WriteLine($"Using PostgreSQL from {source}");
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
app.MapGet("/", (IConfiguration config) => 
{
    var (provider, _, source) = GetDatabaseConfig(config);
    return new
    {
        message = "TodoApi is running!",
        database = provider,
        source = source,
        environment = app.Environment.EnvironmentName,
        endpoints = new[]
        {
            "GET /api/todos - Get all todos",
            "GET /api/todos/{id} - Get todo by ID",
            "POST /api/todos - Create new todo",
            "PUT /api/todos/{id} - Update todo",
            "DELETE /api/todos/{id} - Delete todo"
        },
        swagger = "/swagger"
    };
});

app.Run();

// SMART DATABASE CONFIGURATION RESOLVER - Plug and Play!
static (string provider, string connectionString, string source) GetDatabaseConfig(IConfiguration configuration)
{
    // Priority 1: Railway DATABASE_URL (automatic in production)
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        return ("PostgreSQL", ConvertUriToNpgsql(databaseUrl), "Railway");
    }
    
    // Priority 2: Check DatabaseProvider setting
    var configuredProvider = configuration["DatabaseProvider"];
    
    if (configuredProvider == "SQLite")
    {
        var sqliteConnection = configuration.GetConnectionString("SQLite") ?? "Data Source=todoapi.db";
        return ("SQLite", sqliteConnection, "Local File");
    }
    
    // Priority 3: PostgreSQL - check which source
    var databaseSource = configuration["DatabaseSource"] ?? "Local";
    var postgresConnection = configuration.GetConnectionString(databaseSource);
    
    if (!string.IsNullOrEmpty(postgresConnection))
    {
        // Check if it's URI format (Neon, Supabase, Render provide this)
        if (postgresConnection.StartsWith("postgresql://") || postgresConnection.StartsWith("postgres://"))
        {
            return ("PostgreSQL", ConvertUriToNpgsql(postgresConnection), databaseSource);
        }
        return ("PostgreSQL", postgresConnection, databaseSource); // Already in Npgsql format
    }
    
    // Default: SQLite for local development
    return ("SQLite", "Data Source=todoapi.db", "Local File");
}

// Convert PostgreSQL URI to Npgsql key-value format
static string ConvertUriToNpgsql(string uriString)
{
    // Remove query parameters for parsing
    var uriWithoutQuery = uriString.Split('?')[0];
    var uri = new Uri(uriWithoutQuery);
    var userInfo = uri.UserInfo.Split(':');
    
    // Default to port 5432 if not specified
    var port = uri.Port > 0 ? uri.Port : 5432;
    
    var connectionString = $"Host={uri.Host};Port={port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]}";
    
    // Add SSL mode from query string if present
    if (uriString.Contains("sslmode=require"))
    {
        connectionString += ";SSL Mode=Require";
    }
    else
    {
        connectionString += ";SSL Mode=Prefer";
    }
    
    connectionString += ";Trust Server Certificate=true";
    
    return connectionString;
}


