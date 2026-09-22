namespace Team3.Backend.Features.Users.Dtos;

public class UpdateProfileRequest
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Bio { get; set; }

    public string? University { get; set; }
}