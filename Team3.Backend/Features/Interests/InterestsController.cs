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

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InterestResponse>>>
        GetInterests()
    {
        return Ok(await _interestsService.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InterestResponse>> GetInterest(Guid id)
    {
        var response = await _interestsService.GetByIdAsync(id);

        return response is null
            ? NotFound(new { message = "Interest not found." })
            : Ok(response);
    }

    [HttpGet("/api/profiles/me/interests")]
    [Authorize]
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

    [HttpPut("/api/profiles/me/interests/{interestId:guid}")]
    [Authorize]
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

    [HttpDelete("/api/profiles/me/interests/{interestId:guid}")]
    [Authorize]
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
