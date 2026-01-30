using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Commands.Seed;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
public class SeedController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _env;

    public SeedController(IMediator mediator, IWebHostEnvironment env)
    {
        _mediator = mediator;
        _env = env;
    }

    /// <summary>Seeds the database with sample data (Development only).</summary>
    [HttpPost]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(429)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> SeedDatabase(CancellationToken cancellationToken)
    {
        if (!_env.IsDevelopment())
            return NotFound();

        var result = await _mediator.Send(new SeedDatabaseCommand(), cancellationToken);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(new
        {
            message = result.Message,
            users = result.UsersCreated,
            tags = result.TagsCreated,
            tasks = result.TasksCreated
        });
    }

    /// <summary>Clears seed data (Development only).</summary>
    [HttpDelete]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(429)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ClearDatabase(CancellationToken cancellationToken)
    {
        if (!_env.IsDevelopment())
            return NotFound();

        var result = await _mediator.Send(new ClearDatabaseCommand(), cancellationToken);

        return Ok(new { message = result.Message });
    }
}
