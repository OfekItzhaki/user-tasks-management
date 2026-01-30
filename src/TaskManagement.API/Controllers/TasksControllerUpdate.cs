using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Commands.Tasks;
using TaskManagement.Application.DTOs;

namespace TaskManagement.API.Controllers;

/// <summary>
/// Update task action (partial)
/// </summary>
public partial class TasksController
{
    /// <summary>
    /// Updates an existing task. Task ID is in the URL path, not in the body.
    /// </summary>
    /// <remarks>Priority values: 1=Low, 2=Medium, 3=High, 4=Critical</remarks>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TaskDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(429)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<TaskDto>> UpdateTask(int id, [FromBody] UpdateTaskDto taskDto, CancellationToken cancellationToken = default)
    {
        var command = new UpdateTaskCommand { Id = id, Task = taskDto };
        var updatedTask = await _mediator.Send(command, cancellationToken);
        return Ok(updatedTask);
    }
}
