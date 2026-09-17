namespace Team3.Backend.Features.Progress.Dtos;

public class ProgressResponse
{
    public Guid Id { get; set; }

    public Guid LearningDirectionId { get; set; }

    public string? LearningDirectionName { get; set; }

    public string Level { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
