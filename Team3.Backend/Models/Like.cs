namespace Team3.Backend.Models;

public class Like
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid EducationalContentId { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;

    public EducationalContent EducationalContent { get; set; } = null!;
}
