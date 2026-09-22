using Team3.Backend.Features.Skills.Dtos;
using Team3.Backend.Features.Skills.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.Skills;

public class SkillsService : ISkillsService
{
    private readonly ISkillsRepository _skillsRepository;

    public SkillsService(ISkillsRepository skillsRepository)
    {
        _skillsRepository = skillsRepository;
    }

    public async Task<IReadOnlyList<SkillResponse>> GetAllAsync()
    {
        var skills = await _skillsRepository.GetAllAsync();
        return skills.Select(MapToResponse).ToList();
    }

    public async Task<SkillResponse?> GetByIdAsync(Guid skillId)
    {
        var skill = await _skillsRepository.GetByIdAsync(skillId);
        return skill is null ? null : MapToResponse(skill);
    }

    public async Task<IReadOnlyList<SkillResponse>> GetMySkillsAsync(
        Guid userId)
    {
        var user = await GetUserAsync(userId);
        var skills = user.UserSkills
            .Select(userSkill => userSkill.Skill)
            .OrderBy(skill => skill.Name)
            .ToList();

        return skills.Select(MapToResponse).ToList();
    }

    public async Task<SkillResponse> AddMySkillAsync(
        Guid userId,
        Guid skillId)
    {
        var user = await GetUserAsync(userId);
        var skill = await _skillsRepository.GetByIdAsync(skillId);

        if (skill is null)
        {
            throw new KeyNotFoundException("Skill not found.");
        }

        if (await _skillsRepository.UserHasSkillAsync(user.Id, skillId))
        {
            throw new InvalidOperationException(
                "The skill is already associated with this profile.");
        }

        _skillsRepository.AddUserSkill(new UserSkill
        {
            UserId = user.Id,
            SkillId = skill.Id
        });

        await _skillsRepository.SaveChangesAsync();
        return MapToResponse(skill);
    }

    public async Task<bool> RemoveMySkillAsync(
        Guid userId,
        Guid skillId)
    {
        var user = await GetUserAsync(userId);
        var removed = await _skillsRepository.RemoveUserSkillAsync(
            user.Id,
            skillId);

        if (!removed)
        {
            throw new KeyNotFoundException(
                "The skill is not associated with this profile.");
        }

        await _skillsRepository.SaveChangesAsync();
        return true;
    }

    private async Task<User> GetUserAsync(Guid userId)
    {
        var user = await _skillsRepository
            .GetUserByIdAsync(userId);

        return user ?? throw new KeyNotFoundException("User not found.");
    }

    private static SkillResponse MapToResponse(Skill skill)
    {
        return new SkillResponse
        {
            Id = skill.Id,
            Name = skill.Name,
            Description = skill.Description
        };
    }
}
