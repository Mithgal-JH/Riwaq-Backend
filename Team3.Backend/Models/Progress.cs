namespace Team3.Backend.Models;

public class Progress
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid LearningDirectionId { get; set; }

    public string Level { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;

    public LearningDirection LearningDirection { get; set; } = null!;
}
