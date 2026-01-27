using System.ComponentModel.DataAnnotations;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs;

/// <summary>
/// Data transfer object for updating a task.
/// </summary>
public class UpdateTaskDto
{
    /// <summary>
    /// The ID of the task to update (must match the ID in the URL path).
    /// </summary>
    /// <example>1</example>
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// The title of the task (max 200 characters).
    /// </summary>
    /// <example>Complete project documentation</example>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The description of the task (max 1000 characters).
    /// </summary>
    /// <example>Write comprehensive README and API documentation</example>
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The due date for the task (must be today or in the future).
    /// </summary>
    /// <example>2024-12-31T00:00:00Z</example>
    [Required]
    public DateTime DueDate { get; set; }

    /// <summary>
    /// The priority of the task. Values: 1=Low, 2=Medium, 3=High, 4=Critical
    /// </summary>
    /// <example>3</example>
    [Required]
    [Range(1, 4, ErrorMessage = "Priority must be between 1 and 4")]
    public Priority Priority { get; set; }

    /// <summary>
    /// List of user IDs assigned to this task (at least one required).
    /// </summary>
    /// <example>[1, 2]</example>
    [Required]
    [MinLength(1, ErrorMessage = "At least one user must be assigned")]
    public List<int> UserIds { get; set; } = new();

    /// <summary>
    /// List of tag IDs associated with this task (at least one required).
    /// </summary>
    /// <example>[1, 2, 3]</example>
    [Required]
    [MinLength(1, ErrorMessage = "At least one tag must be selected")]
    public List<int> TagIds { get; set; } = new();
}
