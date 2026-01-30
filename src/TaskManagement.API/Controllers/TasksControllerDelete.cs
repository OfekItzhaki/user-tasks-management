using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Commands.Tasks;

namespace TaskManagement.API.Controllers;

/// <summary>
/// Delete task action (partial)
/// </summary>
public partial class TasksController
{
    /// <summary>
    /// Deletes a task by ID.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(429)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> DeleteTask(int id, CancellationToken cancellationToken = default)
    {
        var command = new DeleteTaskCommand { Id = id };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
