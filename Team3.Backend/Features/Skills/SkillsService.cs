using Team3.Backend.Features.AI.Interfaces;
using Team3.Backend.Features.Skills.Dtos;
using Team3.Backend.Features.Skills.Interfaces;
using Team3.Backend.Models;
using Team3.Backend.Services.Caching;

namespace Team3.Backend.Features.Skills;

public class SkillsService : ISkillsService
{
    private readonly ISkillsRepository _skillsRepository;
    private readonly IProfileSyncService? _profileSyncService;
    private readonly ICacheService _cacheService;

    public SkillsService(
        ISkillsRepository skillsRepository,
        ICacheService cacheService,
        IProfileSyncService? profileSyncService = null)
    {
        _skillsRepository = skillsRepository;
        _profileSyncService = profileSyncService;
        _cacheService = cacheService;
    }

    public async Task<IReadOnlyList<SkillResponse>> GetAllAsync()
    {
        const string cacheKey = "skills:all";

        var cached = await _cacheService.GetAsync<List<SkillResponse>>(cacheKey);

        if (cached is not null)
            return cached;

        var skills = await _skillsRepository.GetAllAsync();
        var result = skills.Select(MapToResponse).ToList();

        await _cacheService.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromMinutes(30));

        return result;
    }

    public async Task<SkillResponse?> GetByIdAsync(Guid skillId)
    {
        var cacheKey = $"skill:{skillId}";

        var cached = await _cacheService.GetAsync<SkillResponse>(cacheKey);

        if (cached is not null)
            return cached;

        var skill = await _skillsRepository.GetByIdAsync(skillId);

        if (skill is null)
            return null;

        var result = MapToResponse(skill);

        await _cacheService.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromMinutes(30));

        return result;
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
        await SyncProfileAsync(userId);
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
        await SyncProfileAsync(userId);
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

    private async Task SyncProfileAsync(Guid userId)
    {
        if (_profileSyncService is not null)
        {
            await _profileSyncService.UpsertProfileAsync(userId);
        }
    }
}
