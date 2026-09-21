namespace Team3.Backend.Features.AI.Dtos;

public sealed class ContentAnalysisRequest
{
    public string RequestId { get; set; } = string.Empty;

    public string ContentId { get; set; } = string.Empty;

    public int ContentVersion { get; set; }

    public string Text { get; set; } = string.Empty;

    public string Language { get; set; } = string.Empty;
}

public sealed class ContentAnalysisResponse
{
    public string RequestId { get; set; } = string.Empty;

    public string ContentId { get; set; } = string.Empty;

    public int ContentVersion { get; set; }

    public string ProcessingStatus { get; set; } = string.Empty;

    public DateTimeOffset ProcessedAt { get; set; }

    public ContentTopics Topics { get; set; } = new();

    public DifficultyResult Difficulty { get; set; } = new();

    public SafetyResult Safety { get; set; } = new();

    public bool NeedsReview { get; set; }

    public ModelVersions ModelVersions { get; set; } = new();

    public string PreprocessingVersion { get; set; } = string.Empty;
}

public sealed class ContentTopics
{
    public string ClassificationStatus { get; set; } = string.Empty;

    public List<TopicResult> PrimaryTopics { get; set; } = [];

    public List<TopicResult> SecondaryTopics { get; set; } = [];

    public int TopicCount { get; set; }
}

public sealed class TopicResult
{
    public string Topic { get; set; } = string.Empty;

    public decimal Confidence { get; set; }
}

public sealed class DifficultyResult
{
    public string Level { get; set; } = string.Empty;

    public decimal Confidence { get; set; }
}

public sealed class SafetyResult
{
    public string Status { get; set; } = string.Empty;

    public List<string> RiskCategories { get; set; } = [];

    public bool ReviewRequired { get; set; }

    public string RecommendationSignal { get; set; } = string.Empty;
}

public sealed class ModelVersions
{
    public string TopicModel { get; set; } = string.Empty;

    public string DifficultyModel { get; set; } = string.Empty;

    public string SafetyModel { get; set; } = string.Empty;
}
