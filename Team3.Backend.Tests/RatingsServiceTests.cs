using FluentAssertions;
using Moq;
using Team3.Backend.Features.Ratings;
using Team3.Backend.Features.Ratings.Dtos;
using Team3.Backend.Features.Ratings.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Tests;

public class RatingsServiceTests
{
    private readonly Mock<IRatingsRepository> _repository = new();
    private readonly RatingsService _service;

    public RatingsServiceTests()
    {
        _service = new RatingsService(_repository.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldSubmitValidRatingSuccessfully()
    {
        var rater = ServiceTestData.User(Guid.NewGuid());
        var rated = ServiceTestData.User(Guid.NewGuid());
        var sessionId = Guid.NewGuid();
        var connection = new Connection
        {
            Id = Guid.NewGuid(),
            UserAId = rater.Id,
            UserBId = rated.Id,
            UserA = rater,
            UserB = rated
        };

        var session = new LearningSession
        {
            Id = sessionId,
            ConnectionId = connection.Id,
            Connection = connection,
            Status = "Completed"
        };

        _repository.Setup(x => x.GetSessionWithConnectionAsync(sessionId))
            .ReturnsAsync(session);
        _repository.Setup(x => x.UserHasRatedSessionAsync(sessionId, rater.Id))
            .ReturnsAsync(false);
        _repository.Setup(x => x.GetUserByIdAsync(rater.Id))
            .ReturnsAsync(rater);
        _repository.Setup(x => x.GetUserByIdAsync(rated.Id))
            .ReturnsAsync(rated);

        var request = new CreateRatingRequest
        {
            Score = 5,
            Review = "Very helpful session."
        };

        var response = await _service.CreateAsync(rater.Id, sessionId, request);

        response.Score.Should().Be(5);
        response.Review.Should().Be("Very helpful session.");
        response.Rater.UserId.Should().Be(rater.Id);
        response.RatedUser.UserId.Should().Be(rated.Id);
        response.LearningSessionId.Should().Be(sessionId);

        _repository.Verify(x => x.Add(It.Is<Rating>(rating =>
            rating.LearningSessionId == sessionId &&
            rating.RaterUserId == rater.Id &&
            rating.RatedUserId == rated.Id &&
            rating.Score == 5)), Times.Once);
        _repository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectScoreBelowOne()
    {
        var rater = ServiceTestData.User(Guid.NewGuid());
        var rated = ServiceTestData.User(Guid.NewGuid());
        var sessionId = Guid.NewGuid();

        var session = new LearningSession
        {
            Id = sessionId,
            Connection = new Connection
            {
                Id = Guid.NewGuid(),
                UserAId = rater.Id,
                UserBId = rated.Id,
                UserA = rater,
                UserB = rated
            },
            Status = "Completed"
        };

        _repository.Setup(x => x.GetSessionWithConnectionAsync(sessionId))
            .ReturnsAsync(session);

        var request = new CreateRatingRequest { Score = 0 };

        var action = () => _service.CreateAsync(rater.Id, sessionId, request);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Score must be between 1 and 5.");
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectScoreAboveFive()
    {
        var rater = ServiceTestData.User(Guid.NewGuid());
        var rated = ServiceTestData.User(Guid.NewGuid());
        var sessionId = Guid.NewGuid();

        var session = new LearningSession
        {
            Id = sessionId,
            Connection = new Connection
            {
                Id = Guid.NewGuid(),
                UserAId = rater.Id,
                UserBId = rated.Id,
                UserA = rater,
                UserB = rated
            },
            Status = "Completed"
        };

        _repository.Setup(x => x.GetSessionWithConnectionAsync(sessionId))
            .ReturnsAsync(session);

        var request = new CreateRatingRequest { Score = 6 };

        var action = () => _service.CreateAsync(rater.Id, sessionId, request);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Score must be between 1 and 5.");
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectMissingSession()
    {
        var rater = ServiceTestData.User(Guid.NewGuid());
        var sessionId = Guid.NewGuid();

        _repository.Setup(x => x.GetSessionWithConnectionAsync(sessionId))
            .ReturnsAsync((LearningSession?)null);

        var request = new CreateRatingRequest { Score = 4 };

        var action = () => _service.CreateAsync(rater.Id, sessionId, request);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Learning session not found.");
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectScheduledSession()
    {
        var rater = ServiceTestData.User(Guid.NewGuid());
        var rated = ServiceTestData.User(Guid.NewGuid());
        var sessionId = Guid.NewGuid();

        var session = new LearningSession
        {
            Id = sessionId,
            Connection = new Connection
            {
                Id = Guid.NewGuid(),
                UserAId = rater.Id,
                UserBId = rated.Id,
                UserA = rater,
                UserB = rated
            },
            Status = "Scheduled"
        };

        _repository.Setup(x => x.GetSessionWithConnectionAsync(sessionId))
            .ReturnsAsync(session);

        var request = new CreateRatingRequest { Score = 5 };

        var action = () => _service.CreateAsync(rater.Id, sessionId, request);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Only completed learning sessions can be rated.");
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectCancelledSession()
    {
        var rater = ServiceTestData.User(Guid.NewGuid());
        var rated = ServiceTestData.User(Guid.NewGuid());
        var sessionId = Guid.NewGuid();

        var session = new LearningSession
        {
            Id = sessionId,
            Connection = new Connection
            {
                Id = Guid.NewGuid(),
                UserAId = rater.Id,
                UserBId = rated.Id,
                UserA = rater,
                UserB = rated
            },
            Status = "Cancelled"
        };

        _repository.Setup(x => x.GetSessionWithConnectionAsync(sessionId))
            .ReturnsAsync(session);

        var request = new CreateRatingRequest { Score = 5 };

        var action = () => _service.CreateAsync(rater.Id, sessionId, request);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Only completed learning sessions can be rated.");
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectUserWhoIsNotSessionParticipant()
    {
        var rater = ServiceTestData.User(Guid.NewGuid());
        var rated = ServiceTestData.User(Guid.NewGuid());
        var outsider = ServiceTestData.User(Guid.NewGuid());
        var sessionId = Guid.NewGuid();

        var session = new LearningSession
        {
            Id = sessionId,
            Connection = new Connection
            {
                Id = Guid.NewGuid(),
                UserAId = rater.Id,
                UserBId = rated.Id,
                UserA = rater,
                UserB = rated
            },
            Status = "Completed"
        };

        _repository.Setup(x => x.GetSessionWithConnectionAsync(sessionId))
            .ReturnsAsync(session);

        var request = new CreateRatingRequest { Score = 4 };

        var action = () => _service.CreateAsync(outsider.Id, sessionId, request);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Only session participants can rate this session.");
    }

    [Fact]
    public async Task CreateAsync_ShouldIdentifyOtherParticipantAsRatedUser()
    {
        var rater = ServiceTestData.User(Guid.NewGuid());
        var rated = ServiceTestData.User(Guid.NewGuid());
        var sessionId = Guid.NewGuid();

        var session = new LearningSession
        {
            Id = sessionId,
            Connection = new Connection
            {
                Id = Guid.NewGuid(),
                UserAId = rater.Id,
                UserBId = rated.Id,
                UserA = rater,
                UserB = rated
            },
            Status = "Completed"
        };

        _repository.Setup(x => x.GetSessionWithConnectionAsync(sessionId))
            .ReturnsAsync(session);
        _repository.Setup(x => x.UserHasRatedSessionAsync(sessionId, rater.Id))
            .ReturnsAsync(false);
        _repository.Setup(x => x.GetUserByIdAsync(rater.Id))
            .ReturnsAsync(rater);
        _repository.Setup(x => x.GetUserByIdAsync(rated.Id))
            .ReturnsAsync(rated);

        var response = await _service.CreateAsync(rater.Id, sessionId, new CreateRatingRequest { Score = 4 });

        response.RatedUser.UserId.Should().Be(rated.Id);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectDuplicateRatingForSameSession()
    {
        var rater = ServiceTestData.User(Guid.NewGuid());
        var rated = ServiceTestData.User(Guid.NewGuid());
        var sessionId = Guid.NewGuid();

        var session = new LearningSession
        {
            Id = sessionId,
            Connection = new Connection
            {
                Id = Guid.NewGuid(),
                UserAId = rater.Id,
                UserBId = rated.Id,
                UserA = rater,
                UserB = rated
            },
            Status = "Completed"
        };

        _repository.Setup(x => x.GetSessionWithConnectionAsync(sessionId))
            .ReturnsAsync(session);
        _repository.Setup(x => x.UserHasRatedSessionAsync(sessionId, rater.Id))
            .ReturnsAsync(true);

        var action = () => _service.CreateAsync(rater.Id, sessionId, new CreateRatingRequest { Score = 5 });

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("You have already rated this learning session.");
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectSelfRating()
    {
        var user = ServiceTestData.User(Guid.NewGuid());
        var sessionId = Guid.NewGuid();
        var connection = new Connection
        {
            Id = Guid.NewGuid(),
            UserAId = user.Id,
            UserBId = user.Id,
            UserA = user,
            UserB = user
        };

        var session = new LearningSession
        {
            Id = sessionId,
            Connection = connection,
            Status = "Completed"
        };

        _repository.Setup(x => x.GetSessionWithConnectionAsync(sessionId))
            .ReturnsAsync(session);

        var action = () => _service.CreateAsync(user.Id, sessionId, new CreateRatingRequest { Score = 3 });

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A user cannot rate themselves.");
    }

    [Fact]
    public async Task GetReceivedByUserAsync_ShouldReturnRatingsReceivedByUser()
    {
        var targetUser = ServiceTestData.User(Guid.NewGuid());
        var rater = ServiceTestData.User(Guid.NewGuid());
        var rating = new Rating
        {
            Id = Guid.NewGuid(),
            LearningSessionId = Guid.NewGuid(),
            RaterUserId = rater.Id,
            RatedUserId = targetUser.Id,
            Score = 5,
            Review = "Great session",
            CreatedAt = DateTime.UtcNow,
            RaterUser = rater,
            RatedUser = targetUser
        };

        _repository.Setup(x => x.GetUserByIdAsync(targetUser.Id)).ReturnsAsync(targetUser);
        _repository.Setup(x => x.GetReceivedRatingsAsync(targetUser.Id)).ReturnsAsync(new List<Rating> { rating });

        var response = await _service.GetReceivedByUserAsync(targetUser.Id);

        response.Should().HaveCount(1);
        response[0].Rater.UserId.Should().Be(rater.Id);
        response[0].RatedUser.UserId.Should().Be(targetUser.Id);
        response[0].Score.Should().Be(5);
    }

    [Fact]
    public async Task GetReceivedByUserAsync_ShouldRejectMissingTargetUser()
    {
        var targetUserId = Guid.NewGuid();

        _repository.Setup(x => x.GetUserByIdAsync(targetUserId)).ReturnsAsync((User?)null);

        var action = () => _service.GetReceivedByUserAsync(targetUserId);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found.");
    }
        [Fact]
    public async Task CreateAsync_ShouldTrimReview()
    {
        var rater = ServiceTestData.User(Guid.NewGuid());
        var rated = ServiceTestData.User(Guid.NewGuid());
        var sessionId = Guid.NewGuid();

        var session = new LearningSession
        {
            Id = sessionId,
            Connection = new Connection
            {
                Id = Guid.NewGuid(),
                UserAId = rater.Id,
                UserBId = rated.Id,
                UserA = rater,
                UserB = rated
            },
            Status = "Completed"
        };

        _repository.Setup(x => x.GetSessionWithConnectionAsync(sessionId))
            .ReturnsAsync(session);
        _repository.Setup(x => x.UserHasRatedSessionAsync(sessionId, rater.Id))
            .ReturnsAsync(false);
        _repository.Setup(x => x.GetUserByIdAsync(rater.Id))
            .ReturnsAsync(rater);
        _repository.Setup(x => x.GetUserByIdAsync(rated.Id))
            .ReturnsAsync(rated);

        var request = new CreateRatingRequest
        {
            Score = 5,
            Review = "  Very helpful session.  "
        };

        var response = await _service.CreateAsync(rater.Id, sessionId, request);

        response.Review.Should().Be("Very helpful session.");
    }

    [Fact]
    public async Task CreateAsync_ShouldAllowRatingWithoutReview()
    {
        var rater = ServiceTestData.User(Guid.NewGuid());
        var rated = ServiceTestData.User(Guid.NewGuid());
        var sessionId = Guid.NewGuid();

        var session = new LearningSession
        {
            Id = sessionId,
            Connection = new Connection
            {
                Id = Guid.NewGuid(),
                UserAId = rater.Id,
                UserBId = rated.Id,
                UserA = rater,
                UserB = rated
            },
            Status = "Completed"
        };

        _repository.Setup(x => x.GetSessionWithConnectionAsync(sessionId))
            .ReturnsAsync(session);
        _repository.Setup(x => x.UserHasRatedSessionAsync(sessionId, rater.Id))
            .ReturnsAsync(false);
        _repository.Setup(x => x.GetUserByIdAsync(rater.Id))
            .ReturnsAsync(rater);
        _repository.Setup(x => x.GetUserByIdAsync(rated.Id))
            .ReturnsAsync(rated);

        var request = new CreateRatingRequest
        {
            Score = 5,
            Review = null
        };

        var response = await _service.CreateAsync(rater.Id, sessionId, request);

        response.Review.Should().BeNull();
        response.Score.Should().Be(5);
    }
}
