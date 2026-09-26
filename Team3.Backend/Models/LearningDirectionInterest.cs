namespace Team3.Backend.Models;

public class LearningDirectionInterest
{
    public Guid LearningDirectionId { get; set; }

    public Guid InterestId { get; set; }

    public LearningDirection LearningDirection { get; set; } = null!;

    public Interest Interest { get; set; } = null!;
}
