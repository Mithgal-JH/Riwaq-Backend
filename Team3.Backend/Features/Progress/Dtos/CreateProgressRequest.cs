namespace Team3.Backend.Features.Progress.Dtos;

public class CreateProgressRequest
{
    public Guid LearningDirectionId { get; set; }

    public string Level { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; }
}
