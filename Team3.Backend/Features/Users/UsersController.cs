using Microsoft.AspNetCore.Mvc;
using Team3.Backend.Features.Users.Dtos;
using Team3.Backend.Features.Users.Interfaces;

namespace Team3.Backend.Features.Users;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
    {
        _usersService = usersService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileResponse>> GetMyProfile()
    {
        var firebaseUid = GetFirebaseUidFromRequest();

        if (firebaseUid is null)
        {
            return BadRequest(new
            {
                message = "X-Firebase-Uid header is required."
            });
        }

        var response = await _usersService
            .GetMyProfileAsync(firebaseUid);

        if (response is null)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        return Ok(response);
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserProfileResponse>> UpdateMyProfile(
        [FromBody] UpdateProfileRequest request)
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
            var response = await _usersService
                .UpdateMyProfileAsync(firebaseUid, request);

            return Ok(response);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new
            {
                message = exception.Message
            });
        }
    }

    [HttpPut("me/learning-direction")]
    public async Task<ActionResult<UserProfileResponse>>
        SelectLearningDirection(
            [FromBody] SelectLearningDirectionRequest request)
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
            var response = await _usersService
                .SelectLearningDirectionAsync(firebaseUid, request);

            return Ok(response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new
            {
                message = exception.Message
            });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PublicUserProfileResponse>> GetPublicProfile(
        Guid id)
    {
        var response = await _usersService.GetPublicProfileAsync(id);

        if (response is null)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        return Ok(response);
    }

    private string? GetFirebaseUidFromRequest()
    {
        var firebaseUid = Request.Headers["X-Firebase-Uid"].ToString();

        return string.IsNullOrWhiteSpace(firebaseUid)
            ? null
            : firebaseUid;
    }
}
