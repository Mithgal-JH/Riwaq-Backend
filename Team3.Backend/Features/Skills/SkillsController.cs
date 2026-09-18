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

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SkillResponse>>> GetSkills()
    {
        return Ok(await _skillsService.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SkillResponse>> GetSkill(Guid id)
    {
        var response = await _skillsService.GetByIdAsync(id);

        return response is null
            ? NotFound(new { message = "Skill not found." })
            : Ok(response);
    }

    [HttpGet("/api/profiles/me/skills")]
    [Authorize]
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

    [HttpPut("/api/profiles/me/skills/{skillId:guid}")]
    [Authorize]
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

    [HttpDelete("/api/profiles/me/skills/{skillId:guid}")]
    [Authorize]
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
