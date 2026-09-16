using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace Team3.Backend.Features.Authentication;

public static class FirebaseAuthenticationService
{
    public static void Initialize()
    {
        // Prevent creating FirebaseApp more than once.
        if (FirebaseApp.DefaultInstance is not null)
        {
            return;
        }

        // In production, Firebase credentials will be stored
        // as a Base64 environment variable.
        var credentialsBase64 = Environment.GetEnvironmentVariable(
            "FIREBASE_CREDENTIALS_BASE64"
        );

        if (!string.IsNullOrWhiteSpace(credentialsBase64))
        {
            // Convert the Base64 string back to the original JSON content.
            var credentialsJson = Convert.FromBase64String(credentialsBase64);

            using var credentialsStream = new MemoryStream(credentialsJson);

            // Initialize Firebase using credentials from memory.
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential
                    .FromStream(credentialsStream)
            });

            return;
        }

        // Local development fallback:
        // Read credentials from a local file path.
        var credentialsPath = Environment.GetEnvironmentVariable(
            "FIREBASE_CREDENTIALS_PATH"
        );

        if (string.IsNullOrWhiteSpace(credentialsPath))
        {
            throw new InvalidOperationException(
                "Firebase credentials are not configured. " +
                "Set FIREBASE_CREDENTIALS_BASE64 for production " +
                "or FIREBASE_CREDENTIALS_PATH for local development."
            );
        }

        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(credentialsPath)
        });
    }
}