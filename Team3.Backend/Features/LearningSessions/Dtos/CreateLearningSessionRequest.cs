namespace Team3.Backend.Features.LearningSessions.Dtos;

public sealed class CreateLearningSessionRequest
{
    public Guid ConnectionId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime ScheduledAt { get; set; }

    public string? MeetingUrl { get; set; }
}