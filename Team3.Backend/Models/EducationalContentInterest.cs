namespace Team3.Backend.Models;

public class EducationalContentInterest
{
    public Guid EducationalContentId { get; set; }

    public Guid InterestId { get; set; }

    public EducationalContent EducationalContent { get; set; } = null!;

    public Interest Interest { get; set; } = null!;
}
