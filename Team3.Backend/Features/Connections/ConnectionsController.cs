using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Team3.Backend.Features.Authentication.Interfaces;
using Team3.Backend.Features.Connections.Dtos;
using Team3.Backend.Features.Connections.Interfaces;

namespace Team3.Backend.Features.Connections;

[ApiController]
[Authorize]
[Route("api/connections")]
public class ConnectionsController : ControllerBase
{
    private readonly IConnectionsService _connectionsService;
    private readonly ICurrentUserService _currentUserService;

    public ConnectionsController(
        IConnectionsService connectionsService,
        ICurrentUserService currentUserService)
    {
        _connectionsService = connectionsService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConnectionResponse>>> GetMyConnections()
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var response = await _connectionsService.GetMyConnectionsAsync(
                userId.Value);

            return Ok(response);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConnectionResponse>> GetById(Guid id)
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var response = await _connectionsService.GetByIdAsync(
                userId.Value,
                id);

            return response is null
                ? NotFound(new { message = "Connection not found." })
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
        catch (InvalidOperationException exception)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new { message = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            await _connectionsService.DeleteAsync(userId.Value, id);
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
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new { message = exception.Message });
        }
    }
}
