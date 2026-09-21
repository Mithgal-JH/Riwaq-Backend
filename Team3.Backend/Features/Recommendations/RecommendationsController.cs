using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Team3.Backend.Features.AI;
using Team3.Backend.Features.Authentication.Interfaces;
using Team3.Backend.Features.Recommendations.Dtos;
using Team3.Backend.Features.Recommendations.Interfaces;

namespace Team3.Backend.Features.Recommendations;

[ApiController]
[Authorize]
[Route("api/recommendations")]
public sealed class RecommendationsController : ControllerBase
{
    private readonly IPersonRecommendationService _recommendationService;
    private readonly ICurrentUserService _currentUserService;

    public RecommendationsController(
        IPersonRecommendationService recommendationService,
        ICurrentUserService currentUserService)
    {
        _recommendationService = recommendationService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Returns personalized people recommendations for the authenticated user.
    /// </summary>
    [HttpGet("people")]
    [ProducesResponseType(typeof(PersonRecommendationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<PersonRecommendationResponse>> GetPeople(
        [FromQuery] int topN = 5,
        [FromQuery] string? requestId = null,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _recommendationService.GetForUserAsync(
                userId.Value,
                topN,
                requestId,
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "User or recommendations not found." });
        }
        catch (AiServiceException exception) when (
            exception.StatusCode == HttpStatusCode.BadRequest)
        {
            return BadRequest(new { message = "Invalid recommendation request." });
        }
        catch (AiServiceException exception) when (
            exception.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound(new { message = "Recommendations are not available." });
        }
        catch (AiServiceException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { message = "Recommendations are temporarily unavailable." });
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { message = "Recommendations are temporarily unavailable." });
        }
        catch (InvalidOperationException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { message = "Recommendations are temporarily unavailable." });
        }
    }
}
