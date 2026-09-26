namespace Team3.Backend.Models;

public class LearningDirection
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();

    public ICollection<Progress> Progresses { get; set; } =
        new List<Progress>();

    public ICollection<LearningDirectionSkill> LearningDirectionSkills { get; set; } =
        new List<LearningDirectionSkill>();

    public ICollection<LearningDirectionInterest> LearningDirectionInterests { get; set; } =
        new List<LearningDirectionInterest>();

    public ICollection<EducationalContentLearningDirection> EducationalContentLearningDirections { get; set; } =
        new List<EducationalContentLearningDirection>();
}
