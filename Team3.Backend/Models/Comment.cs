namespace Team3.Backend.Models;

public class Comment
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid EducationalContentId { get; set; }

    public Guid? ParentCommentId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;

    public EducationalContent EducationalContent { get; set; } = null!;

    public Comment? ParentComment { get; set; }

    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
