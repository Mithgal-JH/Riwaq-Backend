namespace Team3.Backend.Features.LearningSessions.Dtos;

public sealed class LearningSessionResponse
{
    public Guid Id { get; init; }

    public Guid ConnectionId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public DateTime ScheduledAt { get; init; }

    public string? MeetingUrl { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}