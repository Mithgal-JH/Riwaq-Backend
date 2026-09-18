using FluentAssertions;
using Moq;
using Team3.Backend.Features.Users;
using Team3.Backend.Features.Users.Dtos;
using Team3.Backend.Features.Users.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Tests;

public class UsersServiceTests
{
    private readonly Mock<IUsersRepository> _repository = new();
    private readonly UsersService _service;
    private readonly Guid _userId = Guid.NewGuid();

    public UsersServiceTests()
    {
        _service = new UsersService(_repository.Object);
    }

    [Fact]
    public async Task SelectLearningDirectionAsync_ShouldAssignExistingSkillToCurrentUser()
    {
        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = "C#",
            Description = ".NET"
        };
        var user = ServiceTestData.User(_userId);
        _repository.Setup(x => x.GetByIdWithProfileAsync(_userId))
            .ReturnsAsync(user);
        _repository.Setup(x => x.GetSkillByIdAsync(skill.Id))
            .ReturnsAsync(skill);

        var response = await _service.SelectLearningDirectionAsync(_userId,
            new SelectLearningDirectionRequest { SkillId = skill.Id });

        user.LearningDirectionId.Should().Be(skill.Id);
        response.LearningDirectionId.Should().Be(skill.Id);
        _repository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SelectLearningDirectionAsync_ShouldRejectUnknownSkill()
    {
        var user = ServiceTestData.User(_userId);
        var skillId = Guid.NewGuid();
        _repository.Setup(x => x.GetByIdWithProfileAsync(_userId))
            .ReturnsAsync(user);
        _repository.Setup(x => x.GetSkillByIdAsync(skillId))
            .ReturnsAsync((Skill?)null);

        var action = () => _service.SelectLearningDirectionAsync(_userId,
            new SelectLearningDirectionRequest { SkillId = skillId });

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Skill not found.");
        _repository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task SelectLearningDirectionAsync_ShouldRejectEmptySkillId()
    {
        _repository.Setup(x => x.GetByIdWithProfileAsync(_userId))
            .ReturnsAsync(ServiceTestData.User(_userId));

        var action = () => _service.SelectLearningDirectionAsync(_userId,
            new SelectLearningDirectionRequest { SkillId = Guid.Empty });

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("skillId must be a valid Guid.");
        _repository.Verify(x => x.GetSkillByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task UpdateMyProfileAsync_ShouldCreateProfileForCurrentUser()
    {
        var user = ServiceTestData.User(_userId);
        _repository.Setup(x => x.GetByIdWithProfileAsync(_userId))
            .ReturnsAsync(user);

        var response = await _service.UpdateMyProfileAsync(_userId,
            new UpdateProfileRequest
            {
                FirstName = "Ada",
                LastName = "Lovelace",
                Bio = "Developer",
                University = "Analytical University"
            });

        user.Profile.Should().NotBeNull();
        user.Profile!.UserId.Should().Be(_userId);
        user.Profile.FirstName.Should().Be("Ada");
        response.FirstName.Should().Be("Ada");
        _repository.Verify(x => x.AddProfile(It.Is<Profile>(profile =>
            profile.UserId == _userId && profile.LastName == "Lovelace")), Times.Once);
        _repository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
