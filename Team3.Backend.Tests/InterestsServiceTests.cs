using FluentAssertions;
using Moq;
using Team3.Backend.Features.Interests;
using Team3.Backend.Features.Interests.Dtos;
using Team3.Backend.Features.Interests.Interfaces;
using Team3.Backend.Models;
using Team3.Backend.Services.Caching;

namespace Team3.Backend.Tests;

public class InterestsServiceTests
{
    private readonly Mock<IInterestsRepository> _repository = new();
    private readonly Mock<ICacheService> _cacheService = new();
    private readonly InterestsService _service;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _interestId = Guid.NewGuid();

    public InterestsServiceTests()
    {
        _service = new InterestsService(
            _repository.Object,
            _cacheService.Object);
    }

    [Fact]
    public async Task AddMyInterestAsync_ShouldAddInterestToCurrentUser()
    {
        var interest = new Interest { Id = _interestId, Name = "Science" };
        _repository.Setup(x => x.GetUserByIdAsync(_userId))
            .ReturnsAsync(ServiceTestData.User(_userId));
        _repository.Setup(x => x.GetByIdAsync(_interestId))
            .ReturnsAsync(interest);
        _repository.Setup(x => x.UserHasInterestAsync(_userId, _interestId))
            .ReturnsAsync(false);

        var response = await _service.AddMyInterestAsync(_userId, _interestId);

        response.Id.Should().Be(_interestId);
        _repository.Verify(x => x.AddUserInterest(It.Is<UserInterest>(item =>
            item.UserId == _userId && item.InterestId == _interestId)), Times.Once);
        _repository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddMyInterestAsync_ShouldRejectDuplicateInterest()
    {
        _repository.Setup(x => x.GetUserByIdAsync(_userId))
            .ReturnsAsync(ServiceTestData.User(_userId));
        _repository.Setup(x => x.GetByIdAsync(_interestId))
            .ReturnsAsync(new Interest { Id = _interestId, Name = "Science" });
        _repository.Setup(x => x.UserHasInterestAsync(_userId, _interestId))
            .ReturnsAsync(true);

        var action = () => _service.AddMyInterestAsync(_userId, _interestId);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("The interest is already associated with this profile.");
        _repository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task AddMyInterestAsync_ShouldRejectUnknownInterest()
    {
        _repository.Setup(x => x.GetUserByIdAsync(_userId))
            .ReturnsAsync(ServiceTestData.User(_userId));
        _repository.Setup(x => x.GetByIdAsync(_interestId))
            .ReturnsAsync((Interest?)null);

        var action = () => _service.AddMyInterestAsync(_userId, _interestId);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Interest not found.");
    }

    [Fact]
    public async Task RemoveMyInterestAsync_ShouldRemoveOnlyCurrentUsersAssociation()
    {
        _repository.Setup(x => x.GetUserByIdAsync(_userId))
            .ReturnsAsync(ServiceTestData.User(_userId));
        _repository.Setup(x => x.RemoveUserInterestAsync(_userId, _interestId))
            .ReturnsAsync(true);

        var result = await _service.RemoveMyInterestAsync(_userId, _interestId);

        result.Should().BeTrue();
        _repository.Verify(x => x.RemoveUserInterestAsync(_userId, _interestId), Times.Once);
        _repository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveMyInterestAsync_ShouldRejectAssociationOwnedByAnotherUser()
    {
        _repository.Setup(x => x.GetUserByIdAsync(_userId))
            .ReturnsAsync(ServiceTestData.User(_userId));
        _repository.Setup(x => x.RemoveUserInterestAsync(_userId, _interestId))
            .ReturnsAsync(false);

        var action = () => _service.RemoveMyInterestAsync(_userId, _interestId);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("The interest is not associated with this profile.");
        _repository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetMyInterestsAsync_ShouldReturnCurrentUsersInterestsSortedByName()
    {
        var user = ServiceTestData.User(_userId);
        user.UserInterests.Add(new UserInterest
        {
            UserId = _userId,
            InterestId = Guid.NewGuid(),
            Interest = new Interest { Id = Guid.NewGuid(), Name = "Zoology" }
        });
        user.UserInterests.Add(new UserInterest
        {
            UserId = _userId,
            InterestId = Guid.NewGuid(),
            Interest = new Interest { Id = Guid.NewGuid(), Name = "Art" }
        });
        _repository.Setup(x => x.GetUserByIdAsync(_userId)).ReturnsAsync(user);

        var result = await _service.GetMyInterestsAsync(_userId);

        result.Select(interest => interest.Name)
            .Should().Equal("Art", "Zoology");
    }

    [Fact]
    public async Task GetAllAsync_ShouldUseCache_WhenAvailable()
    {
        var cachedInterests = new List<InterestResponse>
        {
            new() { Id = Guid.NewGuid(), Name = "Biology" }
        };
        var cacheService = new Mock<ICacheService>();
        cacheService
            .Setup(x => x.GetAsync<List<InterestResponse>>("interests:all"))
            .ReturnsAsync(cachedInterests);

        var service = new InterestsService(
            _repository.Object,
            cacheService: cacheService.Object);

        var result = await service.GetAllAsync();

        result.Should().BeEquivalentTo(cachedInterests);
        cacheService.Verify(x => x.GetAsync<List<InterestResponse>>("interests:all"), Times.Once);
        _repository.Verify(x => x.GetAllAsync(), Times.Never);
    }
}
