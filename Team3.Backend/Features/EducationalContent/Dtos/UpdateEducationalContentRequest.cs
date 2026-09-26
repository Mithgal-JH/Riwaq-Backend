namespace Team3.Backend.Features.EducationalContent.Dtos;

public class UpdateEducationalContentRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ContentType { get; set; }
    public string? ContentUrl { get; set; }
}