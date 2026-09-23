using FluentAssertions;
using Moq;
using Team3.Backend.Features.AI.Interfaces;
using Team3.Backend.Features.Skills;
using Team3.Backend.Features.Skills.Dtos;
using Team3.Backend.Features.Skills.Interfaces;
using Team3.Backend.Models;
using Team3.Backend.Services.Caching;

namespace Team3.Backend.Tests;

public class SkillsServiceTests
{
    [Fact]
    public async Task GetAllAsync_ShouldUseCache_WhenAvailable()
    {
        var cachedSkills = new List<SkillResponse>
        {
            new() { Id = Guid.NewGuid(), Name = "C#" }
        };
        var repository = new Mock<ISkillsRepository>();
        var cacheService = new Mock<ICacheService>();
        cacheService
            .Setup(x => x.GetAsync<List<SkillResponse>>("skills:all"))
            .ReturnsAsync(cachedSkills);

        var service = new SkillsService(
            repository.Object,
            cacheService: cacheService.Object);

        var result = await service.GetAllAsync();

        result.Should().BeEquivalentTo(cachedSkills);
        cacheService.Verify(x => x.GetAsync<List<SkillResponse>>("skills:all"), Times.Once);
        repository.Verify(x => x.GetAllAsync(), Times.Never);
    }

}
