using Team3.Backend.Features.Users.Dtos;

namespace Team3.Backend.Features.Connections.Dtos;

public class ConnectionResponse
{
    public Guid Id { get; set; }

    public Guid UserAId { get; set; }

    public Guid UserBId { get; set; }

    public DateTime CreatedAt { get; set; }

    public PublicUserProfileResponse UserA { get; set; } = new();

    public PublicUserProfileResponse UserB { get; set; } = new();
}
