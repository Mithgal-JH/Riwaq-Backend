using Team3.Backend.Features.Users.Dtos;

namespace Team3.Backend.Features.Recommendations.Dtos;

public sealed class PersonRecommendationResponse
{
    public string RequestId { get; set; } = string.Empty;

    public string ProfileId { get; set; } = string.Empty;

    public string ProcessingStatus { get; set; } = string.Empty;

    public DateTimeOffset ProcessedAt { get; set; }

    public List<PersonRecommendationItem> Recommendations { get; set; } = [];

    public int RecommendationCount { get; set; }

    public bool LowConfidence { get; set; }

}

public sealed class PersonRecommendationItem
{
    public int Rank { get; set; }

    public PublicUserProfileResponse Profile { get; set; } = new();

    public decimal SimilarityScore { get; set; }

    public List<string> SharedSkills { get; set; } = [];

    public List<string> SharedInterests { get; set; } = [];

    public bool SameLearningDirection { get; set; }
}
