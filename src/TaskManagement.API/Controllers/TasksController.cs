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
[ApiVersion("1.0")]
public partial class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IMediator mediator, ILogger<TasksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>Gets a paged list of tasks with optional filters.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TaskDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(429)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<PagedResult<TaskDto>>> GetTasks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] List<int>? priorities = null,
        [FromQuery] int? userId = null,
        [FromQuery] List<int>? tagIds = null,
        [FromQuery] string? sortBy = "createdAt",
        [FromQuery] string? sortOrder = "desc",
        CancellationToken cancellationToken = default)
    {
        // Validation and sanitization handled in Application layer (FluentValidation + Handler)
        var query = new GetTasksQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            Priorities = priorities,
            UserId = userId,
            TagIds = tagIds,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets a single task by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(429)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<TaskDto>> GetTask(int id, CancellationToken cancellationToken = default)
    {
        var query = new GetTaskByIdQuery { Id = id };
        var task = await _mediator.Send(query, cancellationToken);
        return Ok(task);
    }
}
