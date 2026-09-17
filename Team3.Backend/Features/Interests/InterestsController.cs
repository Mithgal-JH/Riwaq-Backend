using Microsoft.AspNetCore.Mvc;
using Team3.Backend.Features.Interests.Dtos;
using Team3.Backend.Features.Interests.Interfaces;

namespace Team3.Backend.Features.Interests;

[ApiController]
[Route("api/interests")]
public class InterestsController : ControllerBase
{
    private readonly IInterestsService _interestsService;

    public InterestsController(IInterestsService interestsService)
    {
        _interestsService = interestsService;
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
    public async Task<ActionResult<IReadOnlyList<InterestResponse>>>
        GetMyInterests()
    {
        var firebaseUid = GetFirebaseUidFromRequest();

        if (firebaseUid is null)
        {
            return BadRequest(new
            {
                message = "X-Firebase-Uid header is required."
            });
        }

        try
        {
            return Ok(await _interestsService
                .GetMyInterestsAsync(firebaseUid));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpPut("/api/profiles/me/interests/{interestId:guid}")]
    public async Task<ActionResult<InterestResponse>> AddMyInterest(
        Guid interestId)
    {
        var firebaseUid = GetFirebaseUidFromRequest();

        if (firebaseUid is null)
        {
            return BadRequest(new
            {
                message = "X-Firebase-Uid header is required."
            });
        }

        try
        {
            var response = await _interestsService
                .AddMyInterestAsync(firebaseUid, interestId);

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
    public async Task<IActionResult> RemoveMyInterest(Guid interestId)
    {
        var firebaseUid = GetFirebaseUidFromRequest();

        if (firebaseUid is null)
        {
            return BadRequest(new
            {
                message = "X-Firebase-Uid header is required."
            });
        }

        try
        {
            await _interestsService.RemoveMyInterestAsync(
                firebaseUid,
                interestId);

            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    private string? GetFirebaseUidFromRequest()
    {
        var firebaseUid = Request.Headers["X-Firebase-Uid"].ToString();

        return string.IsNullOrWhiteSpace(firebaseUid)
            ? null
            : firebaseUid;
    }
}
