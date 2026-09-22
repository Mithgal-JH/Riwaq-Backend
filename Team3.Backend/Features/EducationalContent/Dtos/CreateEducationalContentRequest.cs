namespace Team3.Backend.Features.EducationalContent.Dtos;

public class CreateEducationalContentRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string ContentType { get; set; } = string.Empty;

    public string? ContentUrl { get; set; }
}