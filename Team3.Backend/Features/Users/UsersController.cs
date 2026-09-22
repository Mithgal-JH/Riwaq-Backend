using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Team3.Backend.Features.Authentication.Interfaces;
using Team3.Backend.Features.Users.Dtos;
using Team3.Backend.Features.Users.Interfaces;

namespace Team3.Backend.Features.Users;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;
    private readonly ICurrentUserService _currentUserService;

    public UsersController(
        IUsersService usersService,
        ICurrentUserService currentUserService)
    {
        _usersService = usersService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Returns the authenticated user's profile.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileResponse>> GetMyProfile()
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        var response = await _usersService
            .GetMyProfileAsync(userId.Value);

        if (response is null)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        return Ok(response);
    }

    /// <summary>
    /// Updates the authenticated user's profile.
    /// </summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileResponse>> UpdateMyProfile(
        [FromBody] UpdateProfileRequest request)
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var response = await _usersService
                .UpdateMyProfileAsync(userId.Value, request);

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

    /// <summary>
    /// Selects the authenticated user's learning direction skill.
    /// </summary>
    [HttpPut("me/learning-direction")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileResponse>>
        SelectLearningDirection(
            [FromBody] SelectLearningDirectionRequest request)
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var response = await _usersService
                .SelectLearningDirectionAsync(userId.Value, request);

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

    /// <summary>
    /// Returns a public user profile by local user ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PublicUserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

}
