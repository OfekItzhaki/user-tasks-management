namespace TaskManagement.WindowsService.Models;

/// <summary>
/// DTO for reminder messages published to and consumed from the reminder queue
/// </summary>
public class ReminderMessage
{
    public int TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    /// <summary>Correlation ID for tracing across publish/consume.</summary>
    public string? CorrelationId { get; set; }
}
