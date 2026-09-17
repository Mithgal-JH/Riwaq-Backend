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

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LearningDirectionResponse>>>
        GetLearningDirections()
    {
        return Ok(await _learningDirectionsService.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LearningDirectionDetailsResponse>>
        GetLearningDirection(Guid id)
    {
        var response = await _learningDirectionsService.GetByIdAsync(id);

        return response is null
            ? NotFound(new { message = "Learning direction not found." })
            : Ok(response);
    }
}
