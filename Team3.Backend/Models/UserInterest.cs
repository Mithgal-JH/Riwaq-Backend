namespace Team3.Backend.Models;

public class UserInterest
{
    public Guid UserId { get; set; }

    public Guid InterestId { get; set; }

    public User User { get; set; } = null!;

    public Interest Interest { get; set; } = null!;
}
