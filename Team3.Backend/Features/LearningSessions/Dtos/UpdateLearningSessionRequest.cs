namespace Team3.Backend.Features.LearningSessions.Dtos;

public sealed class UpdateLearningSessionRequest
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public string? MeetingUrl { get; set; }

    public string? Status { get; set; }
}