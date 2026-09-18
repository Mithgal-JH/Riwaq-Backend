namespace Team3.Backend.Features.Authentication.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }

    string? FirebaseUid { get; }
}
