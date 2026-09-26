using Microsoft.AspNetCore.Mvc;
using Team3.Backend.Features.Comments.DTOs;
using Team3.Backend.Features.Comments.Interfaces;

namespace Team3.Backend.Features.Comments;

[ApiController]
[Route("api")]
public class CommentsController : ControllerBase
{
private readonly ICommentsService _commentsService;


// Store the service used to manage comments.
public CommentsController(ICommentsService commentsService)
{
    _commentsService = commentsService;
}

// Get all comments for educational content.
/// <summary>
/// Returns comments for educational content.
/// </summary>
[HttpGet("educational-content/{educationalContentId:guid}/comments")]
[ProducesResponseType(typeof(List<CommentResponse>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<List<CommentResponse>>> GetByContentId(
    Guid educationalContentId)
{
    // Get all comments for the selected educational content.
    var comments = await _commentsService.GetByContentIdAsync(
        educationalContentId);

    return Ok(comments);
}

// Create a comment for educational content.
/// <summary>
/// Creates a comment using the authenticated Firebase user header.
/// </summary>
[HttpPost("educational-content/{educationalContentId:guid}/comments")]
[ProducesResponseType(typeof(CommentResponse), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<CommentResponse>> Create(
    Guid educationalContentId,
    [FromBody] CreateCommentRequest request)
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
        var response = await _commentsService.CreateAsync(
            firebaseUid,
            educationalContentId,
            request);

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

// Update an existing comment.
/// <summary>
/// Updates a comment owned by the authenticated Firebase user.
/// </summary>
[HttpPatch("comments/{commentId:guid}")]
[ProducesResponseType(typeof(CommentResponse), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<CommentResponse>> Update(
    Guid commentId,
    [FromBody] UpdateCommentRequest request)
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
        var response = await _commentsService.UpdateAsync(
            firebaseUid,
            commentId,
            request);

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
    catch (InvalidOperationException exception)
    {
        return StatusCode(403, new
        {
            message = exception.Message
        });
    }
}

// Delete an existing comment.
/// <summary>
/// Deletes a comment owned by the authenticated Firebase user.
/// </summary>
[HttpDelete("comments/{commentId:guid}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> Delete(Guid commentId)
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
        await _commentsService.DeleteAsync(
            firebaseUid,
            commentId);

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
        return StatusCode(403, new
        {
            message = exception.Message
        });
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
