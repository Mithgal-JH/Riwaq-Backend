using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Team3.Backend.Features.Authentication.Interfaces;
using Team3.Backend.Features.LearningSessions.Dtos;
using Team3.Backend.Features.LearningSessions.Interfaces;

namespace Team3.Backend.Features.LearningSessions;

[ApiController]
[Authorize]
[Route("api/learning-sessions")]
public sealed class LearningSessionsController : ControllerBase
{
    private readonly ILearningSessionsService _service;
    private readonly ICurrentUserService _currentUserService;

    public LearningSessionsController(
        ILearningSessionsService service,
        ICurrentUserService currentUserService)
    {
        _service = service;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LearningSessionResponse>>> GetMine()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        return Ok(await _service.GetMySessionsAsync(userId));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LearningSessionResponse>> GetById(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var response = await _service.GetByIdAsync(userId, id);

            return response is null
                ? NotFound(new { message = "Learning session not found." })
                : Ok(response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<LearningSessionResponse>> Create(
        [FromBody] CreateLearningSessionRequest request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var response = await _service.CreateAsync(userId, request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = response.Id },
                response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = exception.Message
            });
        }
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<LearningSessionResponse>> Update(
        Guid id,
        [FromBody] UpdateLearningSessionRequest request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _service.UpdateAsync(userId, id, request));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    private bool TryGetUserId(out Guid userId)
    {
        userId = _currentUserService.UserId ?? Guid.Empty;
        return userId != Guid.Empty;
    }
}