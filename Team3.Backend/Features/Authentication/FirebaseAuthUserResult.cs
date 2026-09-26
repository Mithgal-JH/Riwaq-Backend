using Team3.Backend.Models;

namespace Team3.Backend.Features.Authentication;

public sealed class FirebaseAuthUserResult
{
    public User User { get; init; } = null!;

    public bool IsNewUser { get; init; }
}
