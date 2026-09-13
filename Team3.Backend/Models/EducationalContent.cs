namespace Team3.Backend.Models;

public class EducationalContent
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string ContentType { get; set; } = string.Empty;

    public string? ContentUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;

    public ICollection<Like> Likes { get; set; } = new List<Like>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<Share> Shares { get; set; } = new List<Share>();

    public ICollection<Save> Saves { get; set; } = new List<Save>();

    public ICollection<Repost> Reposts { get; set; } = new List<Repost>();

    public ICollection<EducationalContentSkill> EducationalContentSkills { get; set; } =
        new List<EducationalContentSkill>();

    public ICollection<EducationalContentInterest> EducationalContentInterests { get; set; } =
        new List<EducationalContentInterest>();

    public ICollection<EducationalContentLearningDirection> EducationalContentLearningDirections { get; set; } =
        new List<EducationalContentLearningDirection>();
}
