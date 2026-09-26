namespace Team3.Backend.Features.AI.Dtos;

public sealed class PostUpsertedRequest
{
    public string PostId { get; set; } = string.Empty;

    public string CreatorId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public string PrimaryTopic { get; set; } = string.Empty;

    public PostTopics Topics { get; set; } = new();

    public DifficultyResult Difficulty { get; set; } = new();

    public PostSafety Safety { get; set; } = new();
}

public sealed class PostTopics
{
    public List<TopicResult> PrimaryTopics { get; set; } = [];
}

public sealed class PostSafety
{
    public string Status { get; set; } = string.Empty;

    public string RecommendationSignal { get; set; } = string.Empty;
}

public sealed class PostUpsertedResponse
{
    public string Status { get; set; } = string.Empty;

    public string PostId { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
