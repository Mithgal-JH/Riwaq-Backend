namespace Team3.Backend.Features.AI.Dtos;

public sealed class PersonRecommendationAiResponse
{
    public string RequestId { get; set; } = string.Empty;

    public string ProfileId { get; set; } = string.Empty;

    public string ProcessingStatus { get; set; } = string.Empty;

    public DateTimeOffset ProcessedAt { get; set; }

    public List<PersonRecommendationAiItem> Recommendations { get; set; } = [];

    public int RecommendationCount { get; set; }

    public bool LowConfidence { get; set; }

    public string ModelVersion { get; set; } = string.Empty;

    public string PreprocessingVersion { get; set; } = string.Empty;
}

public sealed class PersonRecommendationAiItem
{
    public string CandidateProfileId { get; set; } = string.Empty;

    public decimal SimilarityScore { get; set; }

    public List<string> SharedSkills { get; set; } = [];

    public List<string> SharedInterests { get; set; } = [];

    public bool SameLearningDirection { get; set; }
}
