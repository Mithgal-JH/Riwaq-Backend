using Microsoft.AspNetCore.Mvc;
using Team3.Backend.Features.ConnectionRequests.Dtos;
using Team3.Backend.Features.ConnectionRequests.Interfaces;

namespace Team3.Backend.Features.ConnectionRequests;

[ApiController]
[Route("api/connection-requests")]
public class ConnectionRequestsController : ControllerBase
{
    private readonly IConnectionRequestsService _service;

    public ConnectionRequestsController(IConnectionRequestsService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<ConnectionRequestResponse>> Send(
        [FromBody] SendConnectionRequestRequest request)
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
            var response = await _service.SendAsync(firebaseUid, request);
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

    [HttpGet("received")]
    public async Task<ActionResult<List<ConnectionRequestResponse>>> GetReceived()
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
            var response = await _service.GetReceivedAsync(firebaseUid);
            return Ok(response);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpGet("sent")]
    public async Task<ActionResult<List<ConnectionRequestResponse>>> GetSent()
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
            var response = await _service.GetSentAsync(firebaseUid);
            return Ok(response);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpPatch("{connectionRequestId:guid}")]
    public async Task<ActionResult<ConnectionRequestResponse>> UpdateStatus(
        Guid connectionRequestId,
        [FromBody] UpdateConnectionRequestStatusRequest request)
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
            var response = await _service.UpdateStatusAsync(
                firebaseUid,
                connectionRequestId,
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
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = exception.Message
            });
        }
    }

    private string? GetFirebaseUidFromRequest()
    {
        var firebaseUid = Request.Headers["X-Firebase-Uid"].ToString();
        return string.IsNullOrWhiteSpace(firebaseUid) ? null : firebaseUid;
    }
}
