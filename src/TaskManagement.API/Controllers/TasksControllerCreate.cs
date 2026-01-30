using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Commands.Tasks;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Queries.Tasks;

namespace TaskManagement.API.Controllers;

/// <summary>
/// Create task action (partial)
/// </summary>
public partial class TasksController
{
    /// <summary>
    /// Creates a new task.
    /// </summary>
    /// <remarks>Priority values: 1=Low, 2=Medium, 3=High, 4=Critical</remarks>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(429)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto taskDto, CancellationToken cancellationToken = default)
    {
        var command = new CreateTaskCommand { Task = taskDto };
        var createdTask = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask);
    }
}
