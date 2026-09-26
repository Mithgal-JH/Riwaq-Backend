namespace Team3.Backend.Models;

public class Connection
{
    public Guid Id { get; set; }

    public Guid UserAId { get; set; }

    public Guid UserBId { get; set; }

    public DateTime CreatedAt { get; set; }

    public User UserA { get; set; } = null!;

    public User UserB { get; set; } = null!;

    public Conversation? Conversation { get; set; }

    public ICollection<LearningSession> LearningSessions { get; set; } =
        new List<LearningSession>();
}
