using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data;

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options)
        : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Seed some initial data
        modelBuilder.Entity<TodoItem>().HasData(
            new TodoItem 
            { 
                Id = 1, 
                Title = "Learn ASP.NET Core", 
                Description = "Build a REST API with CRUD operations",
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                CompletedAt = DateTime.UtcNow.AddDays(-1)
            },
            new TodoItem 
            { 
                Id = 2, 
                Title = "Deploy to Railway", 
                Description = "Deploy the API to Railway platform",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new TodoItem 
            { 
                Id = 3, 
                Title = "Upgrade to PostgreSQL", 
                Description = "Migrate from SQLite to PostgreSQL",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
