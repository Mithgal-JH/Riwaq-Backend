using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Team3.Backend.Features.Authentication.Interfaces;
using Team3.Backend.Features.SkillVerificationRequests.Dtos;
using Team3.Backend.Features.SkillVerificationRequests.Interfaces;

namespace Team3.Backend.Features.SkillVerificationRequests;

[ApiController]
[Authorize]
[Route("api/skill-verification-requests")]
public sealed class SkillVerificationRequestsController : ControllerBase
{
    private readonly ISkillVerificationRequestsService _service;
    private readonly ICurrentUserService _currentUserService;

    public SkillVerificationRequestsController(
        ISkillVerificationRequestsService service,
        ICurrentUserService currentUserService)
    {
        _service = service;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<ActionResult<SkillVerificationRequestResponse>> Create(
        [FromBody] CreateSkillVerificationRequest request)
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var response = await _service.CreateAsync(userId.Value, request);
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
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpGet("sent")]
    public async Task<ActionResult<IReadOnlyList<SkillVerificationRequestResponse>>> GetSent()
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var response = await _service.GetSentAsync(userId.Value);
            return Ok(response);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpGet("received")]
    public async Task<ActionResult<IReadOnlyList<SkillVerificationRequestResponse>>> GetReceived()
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var response = await _service.GetReceivedAsync(userId.Value);
            return Ok(response);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<SkillVerificationRequestResponse>> Update(
        Guid id,
        [FromBody] UpdateSkillVerificationRequest request)
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var response = await _service.UpdateAsync(userId.Value, id, request);
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
            return Conflict(new { message = exception.Message });
        }
    }
}
