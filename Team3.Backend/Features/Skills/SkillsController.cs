using Microsoft.AspNetCore.Mvc;
using Team3.Backend.Features.Skills.Dtos;
using Team3.Backend.Features.Skills.Interfaces;

namespace Team3.Backend.Features.Skills;

[ApiController]
[Route("api/skills")]
public class SkillsController : ControllerBase
{
    private readonly ISkillsService _skillsService;

    public SkillsController(ISkillsService skillsService)
    {
        _skillsService = skillsService;
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
    public async Task<ActionResult<IReadOnlyList<SkillResponse>>> GetMySkills()
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
            return Ok(await _skillsService.GetMySkillsAsync(firebaseUid));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpPut("/api/profiles/me/skills/{skillId:guid}")]
    public async Task<ActionResult<SkillResponse>> AddMySkill(Guid skillId)
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
            var response = await _skillsService.AddMySkillAsync(
                firebaseUid,
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
    public async Task<IActionResult> RemoveMySkill(Guid skillId)
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
            await _skillsService.RemoveMySkillAsync(firebaseUid, skillId);
            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    private string? GetFirebaseUidFromRequest()
    {
        var firebaseUid = Request.Headers["X-Firebase-Uid"].ToString();

        return string.IsNullOrWhiteSpace(firebaseUid)
            ? null
            : firebaseUid;
    }
}
