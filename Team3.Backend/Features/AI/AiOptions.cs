namespace Team3.Backend.Features.AI;

public sealed class AiOptions
{
    public string? BaseUrl { get; set; }

    public int TimeoutSeconds { get; set; } = 30;
}
