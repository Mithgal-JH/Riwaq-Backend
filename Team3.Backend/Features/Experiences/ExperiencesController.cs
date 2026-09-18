using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Team3.Backend.Features.Authentication.Interfaces;
using Team3.Backend.Features.Experiences.Dtos;
using Team3.Backend.Features.Experiences.Interfaces;

namespace Team3.Backend.Features.Experiences;

[ApiController]
[Authorize]
[Route("api/profiles/me/experiences")]
public class ExperiencesController : ControllerBase
{
    private readonly IExperiencesService _experiencesService;
    private readonly ICurrentUserService _currentUserService;

    public ExperiencesController(
        IExperiencesService experiencesService,
        ICurrentUserService currentUserService)
    {
        _experiencesService = experiencesService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExperienceResponse>>>
        GetMyExperiences()
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _experiencesService
                .GetMyExperiencesAsync(userId.Value));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ExperienceResponse>> Create(
        [FromBody] CreateExperienceRequest request)
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var response = await _experiencesService.CreateAsync(
                userId.Value,
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

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExperienceResponse>> GetById(Guid id)
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var response = await _experiencesService
                .GetByIdAsync(userId.Value, id);

            return response is null
                ? NotFound(new { message = "Experience not found." })
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
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExperienceResponse>> Update(
        Guid id,
        [FromBody] UpdateExperienceRequest request)
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _experiencesService.UpdateAsync(
                userId.Value,
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
            await _experiencesService.DeleteAsync(userId.Value, id);
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

}
