using Team3.Backend.Features.Ratings.Dtos;
using Team3.Backend.Features.Ratings.Interfaces;
using Team3.Backend.Features.Users.Dtos;
using Team3.Backend.Models;

namespace Team3.Backend.Features.Ratings;

public sealed class RatingsService : IRatingsService
{
    private readonly IRatingsRepository _repository;

    public RatingsService(IRatingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<RatingResponse> CreateAsync(
        Guid userId,
        Guid sessionId,
        CreateRatingRequest request)
    {
        ValidateUserId(userId);
        ValidateSessionId(sessionId);
        ArgumentNullException.ThrowIfNull(request);

        if (request.Score < 1 || request.Score > 5)
        {
            throw new ArgumentException("Score must be between 1 and 5.");
        }

        var session = await _repository.GetSessionWithConnectionAsync(sessionId)
            ?? throw new KeyNotFoundException("Learning session not found.");

        if (!string.Equals(session.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only completed learning sessions can be rated.");
        }

        if (session.Connection.UserAId != userId && session.Connection.UserBId != userId)
        {
            throw new InvalidOperationException(
                "Only session participants can rate this session.");
        }

        var ratedUserId = session.Connection.UserAId == userId
            ? session.Connection.UserBId
            : session.Connection.UserAId;

        if (ratedUserId == userId)
        {
            throw new InvalidOperationException(
                "A user cannot rate themselves.");
        }

        if (await _repository.UserHasRatedSessionAsync(sessionId, userId))
        {
            throw new InvalidOperationException(
                "You have already rated this learning session.");
        }

        var raterUser = await _repository.GetUserByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var ratedUser = await _repository.GetUserByIdAsync(ratedUserId)
            ?? throw new KeyNotFoundException("Rated user not found.");

        var review = string.IsNullOrWhiteSpace(request.Review)
            ? null
            : request.Review.Trim();

        var rating = new Rating
        {
            Id = Guid.NewGuid(),
            LearningSessionId = session.Id,
            RaterUserId = userId,
            RatedUserId = ratedUserId,
            Score = request.Score,
            Review = review,
            CreatedAt = DateTime.UtcNow,
            LearningSession = session,
            RaterUser = raterUser,
            RatedUser = ratedUser
        };

        _repository.Add(rating);
        await _repository.SaveChangesAsync();

        return MapToResponse(rating);
    }

    public async Task<IReadOnlyList<RatingResponse>> GetReceivedByUserAsync(
        Guid userId)
    {
        ValidateUserId(userId);

        var targetUser = await _repository.GetUserByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        _ = targetUser;

        var ratings = await _repository.GetReceivedRatingsAsync(userId);

        return ratings
            .Select(MapToResponse)
            .ToList();
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID must be a valid Guid.");
        }
    }

    private static void ValidateSessionId(Guid sessionId)
    {
        if (sessionId == Guid.Empty)
        {
            throw new ArgumentException("Session ID must be a valid Guid.");
        }
    }

    private static RatingResponse MapToResponse(Rating rating)
    {
        return new RatingResponse
        {
            Id = rating.Id,
            Score = rating.Score,
            Review = rating.Review,
            Rater = MapProfile(rating.RaterUser),
            RatedUser = MapProfile(rating.RatedUser),
            LearningSessionId = rating.LearningSessionId,
            CreatedAt = rating.CreatedAt
        };
    }

    private static PublicUserProfileResponse MapProfile(User user)
    {
        return new PublicUserProfileResponse
        {
            UserId = user.Id,
            Points = user.Points,
            LearningDirectionId = user.LearningDirectionId,
            LearningDirectionName = user.SelectedSkill?.Name,
            FirstName = user.Profile?.FirstName,
            LastName = user.Profile?.LastName,
            Bio = user.Profile?.Bio,
            University = user.Profile?.University
        };
    }
}
