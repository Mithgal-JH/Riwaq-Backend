using Team3.Backend.Features.Users.Dtos;

namespace Team3.Backend.Features.Ratings.Dtos;

public sealed class RatingResponse
{
    public Guid Id { get; set; }

    public int Score { get; set; }

    public string? Review { get; set; }

    public PublicUserProfileResponse Rater { get; set; } = new();

    public PublicUserProfileResponse RatedUser { get; set; } = new();

    public Guid LearningSessionId { get; set; }

    public DateTime CreatedAt { get; set; }
}
