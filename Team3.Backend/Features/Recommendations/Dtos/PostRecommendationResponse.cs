using Team3.Backend.Features.EducationalContent.Dtos;

namespace Team3.Backend.Features.Recommendations.Dtos;

public sealed class PostRecommendationResponse
{
    public string RequestId { get; set; } = string.Empty;

    public string? ModelVersion { get; set; }

    public int Count { get; set; }

    public List<PostRecommendationItem> Items { get; set; } = [];
}

public sealed class PostRecommendationItem
{
    public EducationalContentResponse Post { get; set; } = new();

    public int Rank { get; set; }

    public decimal Score { get; set; }

    public List<string> ReasonCodes { get; set; } = [];

    public string PrimaryTopic { get; set; } = string.Empty;
}
