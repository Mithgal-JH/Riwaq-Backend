using FluentAssertions;
using Moq;
using Team3.Backend.Features.Progress;
using Team3.Backend.Features.Progress.Dtos;
using Team3.Backend.Features.Progress.Interfaces;
using ProgressEntity = Team3.Backend.Models.Progress;

namespace Team3.Backend.Tests;

public class ProgressServiceTests
{
    private readonly Mock<IProgressRepository> _repository = new();
    private readonly ProgressService _service;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _directionId = Guid.NewGuid();

    public ProgressServiceTests()
    {
        _service = new ProgressService(_repository.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateProgressForCurrentUser()
    {
        var direction = ServiceTestData.LearningDirection(_directionId);
        _repository.Setup(x => x.GetUserByIdAsync(_userId))
            .ReturnsAsync(ServiceTestData.User(_userId));
        _repository.Setup(x => x.GetLearningDirectionByIdAsync(_directionId))
            .ReturnsAsync(direction);
        _repository.Setup(x => x.ExistsForUserAsync(_userId, _directionId))
            .ReturnsAsync(false);

        var startedAt = DateTime.UtcNow.AddDays(-1);
        var response = await _service.CreateAsync(_userId, new CreateProgressRequest
        {
            LearningDirectionId = _directionId,
            Level = "  Intermediate ",
            StartedAt = startedAt
        });

        response.LearningDirectionId.Should().Be(_directionId);
        response.LearningDirectionName.Should().Be(direction.Name);
        response.Level.Should().Be("Intermediate");
        _repository.Verify(x => x.Add(It.Is<ProgressEntity>(progress =>
            progress.UserId == _userId
            && progress.LearningDirectionId == _directionId
            && progress.Level == "Intermediate")), Times.Once);
        _repository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectDuplicateLearningDirection()
    {
        _repository.Setup(x => x.GetUserByIdAsync(_userId))
            .ReturnsAsync(ServiceTestData.User(_userId));
        _repository.Setup(x => x.GetLearningDirectionByIdAsync(_directionId))
            .ReturnsAsync(ServiceTestData.LearningDirection(_directionId));
        _repository.Setup(x => x.ExistsForUserAsync(_userId, _directionId))
            .ReturnsAsync(true);

        var action = () => _service.CreateAsync(_userId, new CreateProgressRequest
        {
            LearningDirectionId = _directionId,
            Level = "Beginner"
        });

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Progress already exists for this learning direction.");
        _repository.Verify(x => x.Add(It.IsAny<ProgressEntity>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectUnknownLearningDirection()
    {
        _repository.Setup(x => x.GetUserByIdAsync(_userId))
            .ReturnsAsync(ServiceTestData.User(_userId));
        _repository.Setup(x => x.GetLearningDirectionByIdAsync(_directionId))
            .ReturnsAsync((Team3.Backend.Models.LearningDirection?)null);

        var action = () => _service.CreateAsync(_userId, new CreateProgressRequest
        {
            LearningDirectionId = _directionId,
            Level = "Beginner"
        });

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Learning direction not found.");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNullForAnotherUsersProgress()
    {
        var progressId = Guid.NewGuid();
        _repository.Setup(x => x.GetByIdForUserAsync(progressId, _userId))
            .ReturnsAsync((ProgressEntity?)null);

        var response = await _service.GetByIdAsync(_userId, progressId);

        response.Should().BeNull();
        _repository.Verify(x => x.GetByIdForUserAsync(progressId, _userId), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateOwnedProgress()
    {
        var progressId = Guid.NewGuid();
        var progress = ServiceTestData.Progress(progressId, _userId, _directionId);
        _repository.Setup(x => x.GetTrackedByIdForUserAsync(progressId, _userId))
            .ReturnsAsync(progress);
        _repository.Setup(x => x.GetByIdForUserAsync(progressId, _userId))
            .ReturnsAsync(progress);

        var response = await _service.UpdateAsync(_userId, progressId,
            new UpdateProgressRequest { Level = "  Advanced ", StartedAt = DateTime.UtcNow });

        response.Level.Should().Be("Advanced");
        progress.Level.Should().Be("Advanced");
        _repository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldRejectAnotherUsersProgress()
    {
        var progressId = Guid.NewGuid();
        _repository.Setup(x => x.GetTrackedByIdForUserAsync(progressId, _userId))
            .ReturnsAsync((ProgressEntity?)null);

        var action = () => _service.UpdateAsync(_userId, progressId,
            new UpdateProgressRequest { Level = "Advanced" });

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Progress not found.");
        _repository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteOwnedProgress()
    {
        var progressId = Guid.NewGuid();
        var progress = ServiceTestData.Progress(progressId, _userId, _directionId);
        _repository.Setup(x => x.GetTrackedByIdForUserAsync(progressId, _userId))
            .ReturnsAsync(progress);

        await _service.DeleteAsync(_userId, progressId);

        _repository.Verify(x => x.Remove(progress), Times.Once);
        _repository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectMissingLevel()
    {
        var action = () => _service.CreateAsync(_userId, new CreateProgressRequest
        {
            LearningDirectionId = _directionId,
            Level = " "
        });

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Level is required.");
        _repository.Verify(x => x.GetUserByIdAsync(It.IsAny<Guid>()), Times.Never);
    }
}
