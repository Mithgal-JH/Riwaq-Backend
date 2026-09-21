namespace Team3.Backend.Features.AI.Dtos;

public sealed class PostRecommendationRequest
{
    public string RequestId { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public List<string> DeclaredTopics { get; set; } = [];

    public string LearningDirection { get; set; } = string.Empty;

    public List<string> EligibleCandidateIds { get; set; } = [];

    public List<string>? ExcludePostIds { get; set; }

    public int Limit { get; set; } = 10;

    public List<RecentInteractionRequest>? RecentInteractions { get; set; }
}

public sealed class RecentInteractionRequest
{
    public string PostId { get; set; } = string.Empty;

    public string InteractionType { get; set; } = string.Empty;

    public DateTimeOffset Timestamp { get; set; }
}

public sealed class PostRecommendationResponse
{
    public string RequestId { get; set; } = string.Empty;

    public string ModelVersion { get; set; } = string.Empty;

    public int Count { get; set; }

    public List<PostRecommendationItem> Items { get; set; } = [];
}

public sealed class PostRecommendationItem
{
    public string Id { get; set; } = string.Empty;

    public int Rank { get; set; }

    public decimal Score { get; set; }

    public List<string> ReasonCodes { get; set; } = [];

    public string PrimaryTopic { get; set; } = string.Empty;
}
