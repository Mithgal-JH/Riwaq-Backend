namespace Team3.Backend.Models;

public class LearningSession
{
    public Guid Id { get; set; }

    public Guid ConnectionId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime ScheduledAt { get; set; }

    public string? MeetingUrl { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Connection Connection { get; set; } = null!;

    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
