using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Team3.Backend.Features.Authentication.Interfaces;
using Team3.Backend.Features.Progress.Dtos;
using Team3.Backend.Features.Progress.Interfaces;

namespace Team3.Backend.Features.Progress;

[ApiController]
[Authorize]
[Route("api/users/me/progress")]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _progressService;
    private readonly ICurrentUserService _currentUserService;

    public ProgressController(
        IProgressService progressService,
        ICurrentUserService currentUserService)
    {
        _progressService = progressService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Returns progress records owned by the authenticated user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProgressResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProgressResponse>>>
        GetMyProgress()
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _progressService
                .GetMyProgressAsync(userId.Value));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    /// <summary>
    /// Returns an authenticated user's progress record by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProgressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProgressResponse>> GetById(Guid id)
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var response = await _progressService
                .GetByIdAsync(userId.Value, id);

            return response is null
                ? NotFound(new { message = "Progress not found." })
                : Ok(response);
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

    /// <summary>
    /// Creates a progress record for the authenticated user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProgressResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProgressResponse>> Create(
        [FromBody] CreateProgressRequest request)
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var response = await _progressService.CreateAsync(
                userId.Value,
                request);

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
            return Conflict(new { message = exception.Message });
        }
    }

    /// <summary>
    /// Updates a progress record owned by the authenticated user.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProgressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProgressResponse>> Update(
        Guid id,
        [FromBody] UpdateProgressRequest request)
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _progressService.UpdateAsync(
                userId.Value,
                id,
                request));
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

    /// <summary>
    /// Deletes a progress record owned by the authenticated user.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            await _progressService.DeleteAsync(userId.Value, id);
            return NoContent();
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

}
