namespace Team3.Backend.Models;

public class UserExperience
{
    public Guid UserId { get; set; }

    public Guid ExperienceId { get; set; }

    public User User { get; set; } = null!;

    public Experience Experience { get; set; } = null!;
}
