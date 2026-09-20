using Team3.Backend.Features.Ratings.Dtos;

namespace Team3.Backend.Features.Ratings.Interfaces;

public interface IRatingsService
{
    Task<RatingResponse> CreateAsync(
        Guid userId,
        Guid sessionId,
        CreateRatingRequest request);

    Task<IReadOnlyList<RatingResponse>> GetReceivedByUserAsync(Guid userId);
}
