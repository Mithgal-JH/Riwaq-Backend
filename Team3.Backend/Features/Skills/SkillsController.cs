using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Team3.Backend.Features.Authentication.Interfaces;
using Team3.Backend.Features.Skills.Dtos;
using Team3.Backend.Features.Skills.Interfaces;

namespace Team3.Backend.Features.Skills;

[ApiController]
[Route("api/skills")]
public class SkillsController : ControllerBase
{
    private readonly ISkillsService _skillsService;
    private readonly ICurrentUserService _currentUserService;

    public SkillsController(
        ISkillsService skillsService,
        ICurrentUserService currentUserService)
    {
        _skillsService = skillsService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Returns the available standardized skills.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SkillResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SkillResponse>>> GetSkills()
    {
        return Ok(await _skillsService.GetAllAsync());
    }

    /// <summary>
    /// Returns a standardized skill by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SkillResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SkillResponse>> GetSkill(Guid id)
    {
        var response = await _skillsService.GetByIdAsync(id);

        return response is null
            ? NotFound(new { message = "Skill not found." })
            : Ok(response);
    }

    /// <summary>
    /// Returns skills associated with the authenticated user's profile.
    /// </summary>
    [HttpGet("/api/profiles/me/skills")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<SkillResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<SkillResponse>>> GetMySkills()
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _skillsService.GetMySkillsAsync(userId.Value));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    /// <summary>
    /// Associates an existing skill with the authenticated user's profile.
    /// </summary>
    [HttpPut("/api/profiles/me/skills/{skillId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(SkillResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SkillResponse>> AddMySkill(Guid skillId)
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var response = await _skillsService.AddMySkillAsync(
                userId.Value,
                skillId);

            return Ok(response);
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
    /// Removes a skill from the authenticated user's profile.
    /// </summary>
    [HttpDelete("/api/profiles/me/skills/{skillId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMySkill(Guid skillId)
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            await _skillsService.RemoveMySkillAsync(userId.Value, skillId);
            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

}
