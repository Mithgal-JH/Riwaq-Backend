namespace Team3.Backend.Features.Authentication.Dtos;

public class FirebaseLoginRequest
{
    // Firebase ID token received from the frontend.
    public string IdToken { get; set; } = string.Empty;
}