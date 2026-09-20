using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Team3.Backend.Features.Authentication.Interfaces;
using Team3.Backend.Features.Interests.Dtos;
using Team3.Backend.Features.Interests.Interfaces;

namespace Team3.Backend.Features.Interests;

[ApiController]
[Route("api/interests")]
public class InterestsController : ControllerBase
{
    private readonly IInterestsService _interestsService;
    private readonly ICurrentUserService _currentUserService;

    public InterestsController(
        IInterestsService interestsService,
        ICurrentUserService currentUserService)
    {
        _interestsService = interestsService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Returns the available standardized interests.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<InterestResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InterestResponse>>>
        GetInterests()
    {
        return Ok(await _interestsService.GetAllAsync());
    }

    /// <summary>
    /// Returns a standardized interest by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InterestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InterestResponse>> GetInterest(Guid id)
    {
        var response = await _interestsService.GetByIdAsync(id);

        return response is null
            ? NotFound(new { message = "Interest not found." })
            : Ok(response);
    }

    /// <summary>
    /// Returns interests associated with the authenticated user's profile.
    /// </summary>
    [HttpGet("/api/profiles/me/interests")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<InterestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<InterestResponse>>>
        GetMyInterests()
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _interestsService
                .GetMyInterestsAsync(userId.Value));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    /// <summary>
    /// Associates an existing interest with the authenticated user's profile.
    /// </summary>
    [HttpPut("/api/profiles/me/interests/{interestId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(InterestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InterestResponse>> AddMyInterest(
        Guid interestId)
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var response = await _interestsService
                .AddMyInterestAsync(userId.Value, interestId);

            return Ok(response);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    /// <summary>
    /// Removes an interest from the authenticated user's profile.
    /// </summary>
    [HttpDelete("/api/profiles/me/interests/{interestId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMyInterest(Guid interestId)
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            await _interestsService.RemoveMyInterestAsync(
                userId.Value,
                interestId);

            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

}
