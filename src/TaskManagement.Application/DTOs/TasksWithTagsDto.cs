namespace TaskManagement.Application.DTOs;

public class TasksWithTagsDto
{
    // Setters required for EF Core SqlQueryRaw materialization
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int Priority { get; set; }
    public int TagCount { get; set; }
    public string TagNames { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public string AssignedUsers { get; set; } = string.Empty;
}
