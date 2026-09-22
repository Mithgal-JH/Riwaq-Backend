namespace Team3.Backend.Models;

public class Interest
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<UserInterest> UserInterests { get; set; } =
        new List<UserInterest>();

    public ICollection<LearningDirectionInterest> LearningDirectionInterests { get; set; } =
        new List<LearningDirectionInterest>();

    public ICollection<EducationalContentInterest> EducationalContentInterests { get; set; } =
        new List<EducationalContentInterest>();
}
