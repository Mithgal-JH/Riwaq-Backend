namespace Team3.Backend.Features.Ratings.Dtos;

public sealed class CreateRatingRequest
{
    public int Score { get; set; }

    public string? Review { get; set; }
}
