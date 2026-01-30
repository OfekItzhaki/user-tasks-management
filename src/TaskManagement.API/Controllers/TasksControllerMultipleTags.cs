using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Queries.Tasks;
using TasksWithTagsDto = TaskManagement.Application.DTOs.TasksWithTagsDto;

namespace TaskManagement.API.Controllers;

/// <summary>
/// GetTasksWithMultipleTags endpoint (stored procedure) – partial
/// </summary>
public partial class TasksController
{
    /// <summary>
    /// Gets tasks with at least the specified number of tags, sorted by number of tags descending.
    /// Uses stored procedure: GetTasksWithMultipleTags
    /// </summary>
    /// <example>
    /// GET /api/tasks/with-multiple-tags?minTagCount=2
    /// </example>
    [HttpGet("with-multiple-tags")]
    [ProducesResponseType(typeof(List<TasksWithTagsDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(429)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<List<TasksWithTagsDto>>> GetTasksWithMultipleTags(
        [FromQuery] int minTagCount = 2,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTasksWithMultipleTagsQuery { MinTagCount = minTagCount };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
