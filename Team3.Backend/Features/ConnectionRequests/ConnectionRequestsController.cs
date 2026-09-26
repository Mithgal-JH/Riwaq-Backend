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

    /// <summary>
    /// Sends a connection request using the authenticated Firebase user header.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ConnectionRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
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

    /// <summary>
    /// Returns connection requests received by the Firebase user in the request header.
    /// </summary>
    [HttpGet("received")]
    [ProducesResponseType(typeof(List<ConnectionRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Returns connection requests sent by the Firebase user in the request header.
    /// </summary>
    [HttpGet("sent")]
    [ProducesResponseType(typeof(List<ConnectionRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Accepts, rejects, or cancels a connection request according to ownership rules.
    /// </summary>
    [HttpPatch("{connectionRequestId:guid}")]
    [ProducesResponseType(typeof(ConnectionRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
