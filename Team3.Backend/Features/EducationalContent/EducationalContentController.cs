using Microsoft.AspNetCore.Mvc;
using Team3.Backend.Features.EducationalContent.Dtos;
using Team3.Backend.Features.Authentication.Interfaces;
using Team3.Backend.Features.EducationalContent.Interfaces;

namespace Team3.Backend.Features.EducationalContent;

[ApiController]
[Route("api/educational-content")]
public class EducationalContentController : ControllerBase
{
    // Educational Content service.
    private readonly IEducationalContentService _educationalContentService;

    private readonly ICurrentUserService _currentUserService;

    // Educational Content interactions service.
    private readonly IEducationalContentInteractionsService
        _educationalContentInteractionsService;

    public EducationalContentController(
       IEducationalContentService educationalContentService,
       IEducationalContentInteractionsService educationalContentInteractionsService,
       ICurrentUserService currentUserService)
    {
        _educationalContentService = educationalContentService;
        _educationalContentInteractionsService =
            educationalContentInteractionsService;
        _currentUserService = currentUserService;
    }
    // Get all educational content.
    /// <summary>
    /// Returns all educational content.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EducationalContentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EducationalContentResponse>>>
        GetAll()
    {
        var response = await _educationalContentService.GetAllAsync();

        return Ok(response);
    }

    // Get educational content by ID.
    /// <summary>
    /// Returns educational content by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EducationalContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EducationalContentResponse>> GetById(Guid id)
    {
        try
        {
            var response = await _educationalContentService.GetByIdAsync(id);

            return response is null
                ? NotFound(new { message = "Educational content not found." })
                : Ok(response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    // Create new educational content.
    /// <summary>
    /// Creates educational content for the Firebase user in the request header.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EducationalContentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EducationalContentResponse>> Create(
        [FromBody] CreateEducationalContentRequest request)
    {
        var firebaseUid = _currentUserService.FirebaseUid;

        if (string.IsNullOrWhiteSpace(firebaseUid))
        {
            return Unauthorized(new
            {
                message = "Authenticated Firebase user is required."
            });
        }

        try
        {
            var response = await _educationalContentService.CreateAsync(
                firebaseUid,
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
    }

    // Update educational content.
    /// <summary>
    /// Updates educational content owned by the Firebase user in the request header.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(EducationalContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EducationalContentResponse>> Update(
        Guid id,
        [FromBody] UpdateEducationalContentRequest request)
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
            return Ok(await _educationalContentService.UpdateAsync(
                firebaseUid,
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

    // Delete educational content.
    /// <summary>
    /// Deletes educational content owned by the Firebase user in the request header.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
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
            await _educationalContentService.DeleteAsync(
                firebaseUid,
                id);

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

    // Like educational content.
    /// <summary>
    /// Records a like by the Firebase user in the request header.
    /// </summary>
    [HttpPost("{id:guid}/likes")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Like(Guid id)
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
            await _educationalContentInteractionsService.LikeAsync(
                firebaseUid,
                id);

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
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    // Unlike educational content.
    /// <summary>
    /// Removes the Firebase user's like from educational content.
    /// </summary>
    [HttpDelete("{id:guid}/likes")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unlike(Guid id)
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
            await _educationalContentInteractionsService.UnlikeAsync(
                firebaseUid,
                id);

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

    // Save educational content.
    /// <summary>
    /// Saves educational content for the Firebase user in the request header.
    /// </summary>
    [HttpPost("{id:guid}/saves")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Save(Guid id)
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
            await _educationalContentInteractionsService.SaveAsync(
                firebaseUid,
                id);

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
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    // Unsave educational content.
    /// <summary>
    /// Removes the Firebase user's saved state from educational content.
    /// </summary>
    [HttpDelete("{id:guid}/saves")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unsave(Guid id)
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
            await _educationalContentInteractionsService.UnsaveAsync(
                firebaseUid,
                id);

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

    // Repost educational content.
    /// <summary>
    /// Reposts educational content for the Firebase user in the request header.
    /// </summary>
    [HttpPost("{id:guid}/reposts")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Repost(Guid id)
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
            await _educationalContentInteractionsService.RepostAsync(
                firebaseUid,
                id);

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
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    // Undo repost from educational content.
    /// <summary>
    /// Removes the Firebase user's repost from educational content.
    /// </summary>
    [HttpDelete("{id:guid}/reposts")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unrepost(Guid id)
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
            await _educationalContentInteractionsService.UnrepostAsync(
                firebaseUid,
                id);

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
    // Record a share event for educational content.
    /// <summary>
    /// Records a share event for educational content.
    /// </summary>
    [HttpPost("{id:guid}/shares")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Share(Guid id)
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
            await _educationalContentInteractionsService.ShareAsync(
                firebaseUid,
                id);

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

    // Get Firebase UID from request header.
    private string? GetFirebaseUidFromRequest()
    {
        var firebaseUid = Request.Headers["X-Firebase-Uid"].ToString();

        return string.IsNullOrWhiteSpace(firebaseUid)
            ? null
            : firebaseUid;
    }
}