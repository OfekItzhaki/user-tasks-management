using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Commands.Seed.SeedData;

/// <summary>
/// Creates users and tags seed data for database initialization
/// </summary>
public static class SeedDataFactory
{
    public static List<User> CreateUsers()
    {
        return new List<User>
        {
            new User
            {
                FullName = "John Doe",
                Email = "john.doe@example.com",
                Telephone = "555-0101",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                FullName = "Jane Smith",
                Email = "jane.smith@example.com",
                Telephone = "555-0102",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                FullName = "Bob Johnson",
                Email = "bob.johnson@example.com",
                Telephone = "555-0103",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                FullName = "Alice Williams",
                Email = "alice.williams@example.com",
                Telephone = "555-0104",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                FullName = "Charlie Brown",
                Email = "charlie.brown@example.com",
                Telephone = "555-0105",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
    }

    public static List<Tag> CreateTags()
    {
        return new List<Tag>
        {
            new Tag { Name = "Urgent", Color = "#FF0000", CreatedAt = DateTime.UtcNow },
            new Tag { Name = "Important", Color = "#FFA500", CreatedAt = DateTime.UtcNow },
            new Tag { Name = "Development", Color = "#0000FF", CreatedAt = DateTime.UtcNow },
            new Tag { Name = "Testing", Color = "#00FF00", CreatedAt = DateTime.UtcNow },
            new Tag { Name = "Documentation", Color = "#800080", CreatedAt = DateTime.UtcNow },
            new Tag { Name = "Meeting", Color = "#FFC0CB", CreatedAt = DateTime.UtcNow },
            new Tag { Name = "Review", Color = "#FFFF00", CreatedAt = DateTime.UtcNow },
            new Tag { Name = "Bug Fix", Color = "#FF4500", CreatedAt = DateTime.UtcNow }
        };
    }
}
