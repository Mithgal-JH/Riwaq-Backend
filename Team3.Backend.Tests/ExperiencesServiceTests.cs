using FluentAssertions;
using Moq;
using Team3.Backend.Features.Experiences;
using Team3.Backend.Features.Experiences.Dtos;
using Team3.Backend.Features.Experiences.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Tests;

public class ExperiencesServiceTests
{
    private readonly Mock<IExperiencesRepository> _repository = new();
    private readonly ExperiencesService _service;
    private readonly Guid _userId = Guid.NewGuid();

    public ExperiencesServiceTests()
    {
        _service = new ExperiencesService(_repository.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateTrimmedExperienceForCurrentUser()
    {
        _repository.Setup(x => x.GetUserByIdAsync(_userId))
            .ReturnsAsync(ServiceTestData.User(_userId));

        var response = await _service.CreateAsync(_userId, new CreateExperienceRequest
        {
            Title = "  My project  ",
            Description = "Details"
        });

        response.UserId.Should().Be(_userId);
        response.Title.Should().Be("My project");
        _repository.Verify(x => x.Add(It.Is<Experience>(experience =>
            experience.UserId == _userId
            && experience.Title == "My project"
            && experience.Description == "Details")), Times.Once);
        _repository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectMissingTitle()
    {
        var action = () => _service.CreateAsync(_userId, new CreateExperienceRequest
        {
            Title = "   "
        });

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Title is required.");
        _repository.Verify(x => x.Add(It.IsAny<Experience>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOnlyExperienceOwnedByCurrentUser()
    {
        var experienceId = Guid.NewGuid();
        var experience = ServiceTestData.Experience(experienceId, _userId);
        _repository.Setup(x => x.GetByIdForUserAsync(experienceId, _userId))
            .ReturnsAsync(experience);

        var response = await _service.GetByIdAsync(_userId, experienceId);

        response.Should().NotBeNull();
        response!.UserId.Should().Be(_userId);
        _repository.Verify(x => x.GetByIdForUserAsync(experienceId, _userId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNullForAnotherUsersExperience()
    {
        var experienceId = Guid.NewGuid();
        _repository.Setup(x => x.GetByIdForUserAsync(experienceId, _userId))
            .ReturnsAsync((Experience?)null);

        var response = await _service.GetByIdAsync(_userId, experienceId);

        response.Should().BeNull();
        _repository.Verify(x => x.GetByIdForUserAsync(experienceId, _userId), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldRejectAnotherUsersExperience()
    {
        var experienceId = Guid.NewGuid();
        _repository.Setup(x => x.GetByIdForUserAsync(experienceId, _userId))
            .ReturnsAsync((Experience?)null);

        var action = () => _service.UpdateAsync(_userId, experienceId,
            new UpdateExperienceRequest { Title = "Changed" });

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Experience not found.");
        _repository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteOwnedExperience()
    {
        var experienceId = Guid.NewGuid();
        var experience = ServiceTestData.Experience(experienceId, _userId);
        _repository.Setup(x => x.GetByIdForUserAsync(experienceId, _userId))
            .ReturnsAsync(experience);

        await _service.DeleteAsync(_userId, experienceId);

        _repository.Verify(x => x.Remove(experience), Times.Once);
        _repository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRejectEmptyExperienceId()
    {
        var action = () => _service.DeleteAsync(_userId, Guid.Empty);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Experience ID must be a valid Guid.");
        _repository.Verify(x => x.Remove(It.IsAny<Experience>()), Times.Never);
    }
}
