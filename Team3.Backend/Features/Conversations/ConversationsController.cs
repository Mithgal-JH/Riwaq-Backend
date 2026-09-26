using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Team3.Backend.Features.Authentication.Interfaces;
using Team3.Backend.Features.Conversations.Dtos;
using Team3.Backend.Features.Conversations.Interfaces;

namespace Team3.Backend.Features.Conversations;

[ApiController]
[Authorize]
[Route("api/conversations")]
public sealed class ConversationsController : ControllerBase
{
    private readonly IConversationsService _service;
    private readonly ICurrentUserService _currentUserService;

    public ConversationsController(
        IConversationsService service,
        ICurrentUserService currentUserService)
    {
        _service = service;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConversationResponse>>> GetMyConversations()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        return Ok(await _service.GetMyConversationsAsync(userId));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConversationResponse>> GetById(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var response = await _service.GetConversationByIdAsync(userId, id);

            return response is null
                ? NotFound(new { message = "Conversation not found." })
                : Ok(response);
        }
        catch (InvalidOperationException exception)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new { message = exception.Message });
        }
    }

    [HttpGet("{conversationId:guid}/messages")]
    public async Task<ActionResult<IReadOnlyList<MessageResponse>>> GetMessages(
        Guid conversationId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var response = await _service.GetMessagesAsync(userId, conversationId);
            return Ok(response);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new { message = exception.Message });
        }
    }

    [HttpPost("{conversationId:guid}/messages")]
    public async Task<ActionResult<MessageResponse>> SendMessage(
        Guid conversationId,
        [FromBody] SendMessageRequest request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var response = await _service.SendMessageAsync(
                userId,
                conversationId,
                request);

            return CreatedAtAction(
                nameof(GetMessages),
                new { conversationId = conversationId },
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
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new { message = exception.Message });
        }
    }

    [HttpPatch("/messages/{id:guid}")]
    public async Task<ActionResult<MessageResponse>> UpdateMessage(
        Guid id,
        [FromBody] UpdateMessageRequest request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var response = await _service.UpdateMessageAsync(userId, id, request);
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
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new { message = exception.Message });
        }
    }

    private bool TryGetUserId(out Guid userId)
    {
        userId = _currentUserService.UserId ?? Guid.Empty;
        return userId != Guid.Empty;
    }
}
