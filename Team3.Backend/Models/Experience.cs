namespace Team3.Backend.Models;

public class Experience
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<UserExperience> UserExperiences { get; set; } =
        new List<UserExperience>();
}
