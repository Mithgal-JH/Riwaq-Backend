using Microsoft.AspNetCore.Mvc;
using Team3.Backend.Features.LearningDirections.Dtos;
using Team3.Backend.Features.LearningDirections.Interfaces;

namespace Team3.Backend.Features.LearningDirections;

[ApiController]
[Route("api/learning-directions")]
public class LearningDirectionsController : ControllerBase
{
    private readonly ILearningDirectionsService _learningDirectionsService;

    public LearningDirectionsController(
        ILearningDirectionsService learningDirectionsService)
    {
        _learningDirectionsService = learningDirectionsService;
    }

    /// <summary>
    /// Returns all available learning directions.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<LearningDirectionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LearningDirectionResponse>>>
        GetLearningDirections()
    {
        return Ok(await _learningDirectionsService.GetAllAsync());
    }

    /// <summary>
    /// Returns a learning direction and its related skills by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LearningDirectionDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LearningDirectionDetailsResponse>>
        GetLearningDirection(Guid id)
    {
        var response = await _learningDirectionsService.GetByIdAsync(id);

        return response is null
            ? NotFound(new { message = "Learning direction not found." })
            : Ok(response);
    }
}
