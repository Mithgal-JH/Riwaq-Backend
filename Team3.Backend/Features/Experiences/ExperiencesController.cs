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

    /// <summary>
    /// Returns experiences owned by the authenticated user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ExperienceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Creates an experience for the authenticated user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ExperienceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Returns an experience owned by the authenticated user.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ExperienceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Updates an experience owned by the authenticated user.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ExperienceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Deletes an experience owned by the authenticated user.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
