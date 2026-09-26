namespace Team3.Backend.Models;

public class Rating
{
    public Guid Id { get; set; }

    public Guid LearningSessionId { get; set; }

    public Guid RaterUserId { get; set; }

    public Guid RatedUserId { get; set; }

    public int Score { get; set; }

    public string? Review { get; set; }

    public DateTime CreatedAt { get; set; }

    public LearningSession LearningSession { get; set; } = null!;

    public User RaterUser { get; set; } = null!;

    public User RatedUser { get; set; } = null!;
}
