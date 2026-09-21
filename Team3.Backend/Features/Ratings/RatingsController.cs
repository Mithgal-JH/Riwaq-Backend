using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Team3.Backend.Features.Authentication.Interfaces;
using Team3.Backend.Features.Ratings.Dtos;
using Team3.Backend.Features.Ratings.Interfaces;

namespace Team3.Backend.Features.Ratings;

[ApiController]
[Authorize]
[Route("api")]
public sealed class RatingsController : ControllerBase
{
    private readonly IRatingsService _service;
    private readonly ICurrentUserService _currentUserService;

    public RatingsController(
        IRatingsService service,
        ICurrentUserService currentUserService)
    {
        _service = service;
        _currentUserService = currentUserService;
    }

    [HttpPost("learning-sessions/{sessionId:guid}/ratings")]
    public async Task<ActionResult<RatingResponse>> CreateRating(
        Guid sessionId,
        [FromBody] CreateRatingRequest request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var response = await _service.CreateAsync(userId, sessionId, request);
            return CreatedAtAction(nameof(GetRatingsForUser), new { userId = response.RatedUser.UserId }, response);
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
            return StatusCode(StatusCodes.Status403Forbidden, new { message = exception.Message });
        }
    }

    [HttpGet("profiles/{userId:guid}/ratings")]
    public async Task<ActionResult<IReadOnlyList<RatingResponse>>> GetRatingsForUser(
        Guid userId)
    {
        if (!TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        _ = currentUserId;

        try
        {
            var response = await _service.GetReceivedByUserAsync(userId);
            return Ok(response);
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
