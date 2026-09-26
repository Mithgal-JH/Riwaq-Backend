namespace Team3.Backend.Models;

public class ContentAnalysis
{
    public Guid Id { get; set; }

    public Guid EducationalContentId { get; set; }

    public int ContentVersion { get; set; }

    public string RequestId { get; set; } = string.Empty;

    public string AnalysisState { get; set; } = "Pending";

    public string? ProcessingStatus { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }

    public string? ClassificationStatus { get; set; }

    public string? TopicReasonCode { get; set; }

    public string PrimaryTopicsJson { get; set; } = "[]";

    public string SecondaryTopicsJson { get; set; } = "[]";

    public int TopicCount { get; set; }

    public string? DifficultyLevel { get; set; }

    public decimal? DifficultyConfidence { get; set; }

    public string? SafetyStatus { get; set; }

    public decimal? SafetyConfidence { get; set; }

    public decimal? SafetyThreshold { get; set; }

    public bool SafetyReviewRequired { get; set; }

    public string RiskCategoriesJson { get; set; } = "[]";

    public string? RecommendationSignal { get; set; }

    public bool NeedsReview { get; set; }

    public string? TopicModel { get; set; }

    public string? DifficultyModel { get; set; }

    public string? SafetyModel { get; set; }

    public string? PreprocessingVersion { get; set; }

    public string? FailureCode { get; set; }

    public EducationalContent EducationalContent { get; set; } = null!;
}
