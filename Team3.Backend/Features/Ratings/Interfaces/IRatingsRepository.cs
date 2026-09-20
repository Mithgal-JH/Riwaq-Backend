using Team3.Backend.Models;

namespace Team3.Backend.Features.Ratings.Interfaces;

public interface IRatingsRepository
{
    Task<User?> GetUserByIdAsync(Guid userId);

    Task<LearningSession?> GetSessionWithConnectionAsync(Guid learningSessionId);

    Task<bool> UserHasRatedSessionAsync(Guid learningSessionId, Guid userId);

    void Add(Rating rating);

    Task<List<Rating>> GetReceivedRatingsAsync(Guid userId);

    Task SaveChangesAsync();
}
