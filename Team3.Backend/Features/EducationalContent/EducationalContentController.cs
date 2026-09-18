using Microsoft.AspNetCore.Mvc;
using Team3.Backend.Features.EducationalContent.Dtos;
using Team3.Backend.Features.EducationalContent.Interfaces;

namespace Team3.Backend.Features.EducationalContent;

[ApiController]
[Route("api/educational-content")]
public class EducationalContentController : ControllerBase
{
    // Educational Content service.
    private readonly IEducationalContentService _educationalContentService;

    // Educational Content interactions service.
    private readonly IEducationalContentInteractionsService
        _educationalContentInteractionsService;

    public EducationalContentController(
        IEducationalContentService educationalContentService,
        IEducationalContentInteractionsService educationalContentInteractionsService)
    {
        _educationalContentService = educationalContentService;
        _educationalContentInteractionsService =
            educationalContentInteractionsService;
    }

    // Get all educational content.
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EducationalContentResponse>>>
        GetAll()
    {
        var response = await _educationalContentService.GetAllAsync();

        return Ok(response);
    }

    // Get educational content by ID.
    [HttpGet("{id:guid}")]
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
    [HttpPost]
    public async Task<ActionResult<EducationalContentResponse>> Create(
        [FromBody] CreateEducationalContentRequest request)
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
    [HttpPatch("{id:guid}")]
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
    [HttpDelete("{id:guid}")]
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
    [HttpPost("{id:guid}/likes")]
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
    [HttpDelete("{id:guid}/likes")]
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
    [HttpPost("{id:guid}/saves")]
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
    [HttpDelete("{id:guid}/saves")]
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
    [HttpPost("{id:guid}/reposts")]
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
    [HttpDelete("{id:guid}/reposts")]
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
    [HttpPost("{id:guid}/shares")]
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