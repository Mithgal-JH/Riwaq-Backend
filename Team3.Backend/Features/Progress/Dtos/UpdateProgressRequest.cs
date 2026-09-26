namespace Team3.Backend.Features.Progress.Dtos;

public class UpdateProgressRequest
{
    public string Level { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; }
}
