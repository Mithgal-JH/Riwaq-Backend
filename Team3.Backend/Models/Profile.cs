namespace Team3.Backend.Models;

public class Profile
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Bio { get; set; }

    public string? University { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}
