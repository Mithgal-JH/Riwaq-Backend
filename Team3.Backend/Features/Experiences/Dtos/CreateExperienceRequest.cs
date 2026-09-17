namespace Team3.Backend.Features.Experiences.Dtos;

public class CreateExperienceRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
}
