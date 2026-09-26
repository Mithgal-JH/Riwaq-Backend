using FluentAssertions;
using Moq;
using Team3.Backend.Services.Caching;
using Team3.Backend.Features.LearningDirections;
using Team3.Backend.Features.LearningDirections.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Tests;

public class LearningDirectionsServiceTests
{
    [Fact]
    public async Task GetAllAsync_ShouldMapDirectionsInRepositoryOrder()
    {
        var first = ServiceTestData.LearningDirection(Guid.NewGuid());
        var second = ServiceTestData.LearningDirection(Guid.NewGuid());
        second.Name = "Frontend Development";
        var repository = new Mock<ILearningDirectionsRepository>();
        repository.Setup(x => x.GetAllAsync())
            .ReturnsAsync([first, second]);
        var cacheService = new Mock<ICacheService>();
        var service = new LearningDirectionsService(
            repository.Object,
            cacheService.Object);
        var result = await service.GetAllAsync();

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(first.Id);
        result[1].Name.Should().Be("Frontend Development");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldMapSkillsSortedByName()
    {
        var direction = ServiceTestData.LearningDirection(Guid.NewGuid());
        direction.LearningDirectionSkills.Add(new LearningDirectionSkill
        {
            LearningDirectionId = direction.Id,
            Skill = new Skill { Id = Guid.NewGuid(), Name = "Zig" }
        });
        direction.LearningDirectionSkills.Add(new LearningDirectionSkill
        {
            LearningDirectionId = direction.Id,
            Skill = new Skill { Id = Guid.NewGuid(), Name = "Algorithms" }
        });
        var repository = new Mock<ILearningDirectionsRepository>();
        repository.Setup(x => x.GetByIdWithSkillsAsync(direction.Id))
            .ReturnsAsync(direction);
        var cacheService = new Mock<ICacheService>();

        var service = new LearningDirectionsService(
            repository.Object,
            cacheService.Object);
        var result = await service.GetByIdAsync(direction.Id);

        result.Should().NotBeNull();
        result!.Skills.Select(skill => skill.Name)
            .Should().Equal("Algorithms", "Zig");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNullForUnknownDirection()
    {
        var repository = new Mock<ILearningDirectionsRepository>();
        var directionId = Guid.NewGuid();
        repository.Setup(x => x.GetByIdWithSkillsAsync(directionId))
            .ReturnsAsync((LearningDirection?)null);
        var cacheService = new Mock<ICacheService>();
        var service = new LearningDirectionsService(
            repository.Object,
            cacheService.Object);
        (await service.GetByIdAsync(directionId)).Should().BeNull();
    }
}
