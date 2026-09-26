namespace Team3.Backend.Features.Experiences.Dtos;

public class ExperienceResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
}
