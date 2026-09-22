namespace Team3.Backend.Models;

public class Experience
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public User User { get; set; } = null!;
}