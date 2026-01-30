using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Queries.Tags;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
public class TagsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TagsController> _logger;

    public TagsController(IMediator mediator, ILogger<TagsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>Gets all tags.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TagDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(429)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<List<TagDto>>> GetTags(CancellationToken cancellationToken = default)
    {
        var query = new GetTagsQuery();
        var tags = await _mediator.Send(query, cancellationToken);
        return Ok(tags);
    }
}
