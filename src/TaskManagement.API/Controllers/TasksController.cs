using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Commands.Tasks;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Queries.Tasks;
using PagedResult = TaskManagement.Application.Queries.Tasks.PagedResult<TaskManagement.Application.DTOs.TaskDto>;
using TasksWithTagsDto = TaskManagement.Application.DTOs.TasksWithTagsDto;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IMediator mediator, ILogger<TasksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TaskDto>>> GetTasks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int? priority = null, // Single priority (backward compatibility)
        [FromQuery] List<int>? priorities = null, // Multiple priorities
        [FromQuery] int? userId = null,
        [FromQuery] int? tagId = null, // Single tag (backward compatibility)
        [FromQuery] List<int>? tagIds = null, // Multiple tags
        [FromQuery] string? sortBy = "createdAt",
        [FromQuery] string? sortOrder = "desc")
    {
        try
        {
            // Validate and sanitize inputs
            if (page < 1)
            {
                return BadRequest("Page must be greater than 0.");
            }

            if (pageSize < 1)
            {
                return BadRequest("Page size must be greater than 0.");
            }

            if (pageSize > 1000)
            {
                return BadRequest("Page size cannot exceed 1000.");
            }

            // Validate negative IDs
            if (priority.HasValue && priority.Value < 1)
            {
                return BadRequest("Priority must be greater than 0.");
            }

            if (priorities != null && priorities.Any(p => p < 1))
            {
                return BadRequest("All priorities must be greater than 0.");
            }

            if (userId.HasValue && userId.Value < 1)
            {
                return BadRequest("User ID must be greater than 0.");
            }

            if (tagId.HasValue && tagId.Value < 1)
            {
                return BadRequest("Tag ID must be greater than 0.");
            }

            if (tagIds != null && tagIds.Any(id => id < 1))
            {
                return BadRequest("All tag IDs must be greater than 0.");
            }

            // Sanitize string inputs (trim whitespace, handle empty strings)
            var sanitizedSearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();
            var sanitizedSortBy = string.IsNullOrWhiteSpace(sortBy) ? "createdAt" : sortBy.Trim();
            var sanitizedSortOrder = string.IsNullOrWhiteSpace(sortOrder) ? "desc" : sortOrder.Trim().ToLower();

            // Validate sort fields
            var validSortFields = new[] { "title", "duedate", "priority", "createdat" };
            if (!validSortFields.Contains(sanitizedSortBy.ToLower()))
            {
                return BadRequest($"SortBy must be one of: {string.Join(", ", validSortFields)}.");
            }

            if (sanitizedSortOrder != "asc" && sanitizedSortOrder != "desc")
            {
                return BadRequest("SortOrder must be 'asc' or 'desc'.");
            }

            var query = new GetTasksQuery
            {
                Page = page,
                PageSize = pageSize,
                SearchTerm = sanitizedSearchTerm,
                Priority = priority,
                Priorities = priorities,
                UserId = userId,
                TagId = tagId,
                TagIds = tagIds,
                SortBy = sanitizedSortBy,
                SortOrder = sanitizedSortOrder
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error retrieving tasks");
            return BadRequest(ex.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks");
            return StatusCode(500, "An error occurred while retrieving tasks.");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> GetTask(int id)
    {
        try
        {
            var query = new GetTaskByIdQuery { Id = id };
            var task = await _mediator.Send(query);
            return Ok(task);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Task not found: {TaskId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task {TaskId}", id);
            return StatusCode(500, "An error occurred while retrieving the task.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto taskDto)
    {
        try
        {
            var command = new CreateTaskCommand { Task = taskDto };
            var createdTask = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask);
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error creating task");
            return BadRequest(ex.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return StatusCode(500, "An error occurred while creating the task.");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TaskDto>> UpdateTask(int id, [FromBody] UpdateTaskDto taskDto)
    {
        try
        {
            if (id != taskDto.Id)
            {
                return BadRequest("Task ID mismatch.");
            }

            var command = new UpdateTaskCommand { Task = taskDto };
            var updatedTask = await _mediator.Send(command);
            return Ok(updatedTask);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Task not found: {TaskId}", id);
            return NotFound(ex.Message);
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error updating task {TaskId}", id);
            return BadRequest(ex.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", id);
            return StatusCode(500, "An error occurred while updating the task.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTask(int id)
    {
        try
        {
            var command = new DeleteTaskCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Task not found: {TaskId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", id);
            return StatusCode(500, "An error occurred while deleting the task.");
        }
    }

    /// <summary>
    /// Gets tasks with at least the specified number of tags, sorted by number of tags descending.
    /// This endpoint executes the stored procedure: GetTasksWithMultipleTags
    /// </summary>
    /// <param name="minTagCount">Minimum number of tags required (default: 2)</param>
    /// <returns>List of tasks with tag counts and aggregated tag names</returns>
    /// <response code="200">Returns the list of tasks with multiple tags</response>
    /// <response code="500">If an error occurred while retrieving tasks</response>
    /// <example>
    /// GET /api/tasks/with-multiple-tags?minTagCount=2
    /// 
    /// Response:
    /// [
    ///   {
    ///     "id": 1,
    ///     "title": "Task with Multiple Tags",
    ///     "description": "This task has multiple tags",
    ///     "dueDate": "2024-12-31T00:00:00Z",
    ///     "priority": 2,
    ///     "tagCount": 3,
    ///     "tagNames": "Backend, Frontend, Urgent",
    ///     "userCount": 2,
    ///     "assignedUsers": "John Doe, Jane Smith"
    ///   }
    /// ]
    /// </example>
    [HttpGet("with-multiple-tags")]
    [ProducesResponseType(typeof(List<TasksWithTagsDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<List<TasksWithTagsDto>>> GetTasksWithMultipleTags(
        [FromQuery] int minTagCount = 2)
    {
        try
        {
            var query = new GetTasksWithMultipleTagsQuery { MinTagCount = minTagCount };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks with multiple tags: {Message}", ex.Message);
            _logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);
            return StatusCode(500, $"An error occurred while retrieving tasks with multiple tags. Error: {ex.Message}");
        }
    }
}
