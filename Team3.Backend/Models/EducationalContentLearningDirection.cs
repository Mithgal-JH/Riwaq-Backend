namespace Team3.Backend.Models;

public class EducationalContentLearningDirection
{
    public Guid EducationalContentId { get; set; }

    public Guid LearningDirectionId { get; set; }

    public EducationalContent EducationalContent { get; set; } = null!;

    public LearningDirection LearningDirection { get; set; } = null!;
}
