using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Identity;
using Team3.Backend.Models;

namespace Team3.Backend.Features.Authentication;

public class FirebaseAuthenticationService
{
    private readonly UserManager<User> _userManager;

    public FirebaseAuthenticationService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public static void Initialize()
    {
        if (FirebaseApp.DefaultInstance is not null)
        {
            return;
        }

        var credentialsBase64 = Environment.GetEnvironmentVariable(
            "FIREBASE_CREDENTIALS_BASE64"
        );

        if (!string.IsNullOrWhiteSpace(credentialsBase64))
        {
            var credentialsJson = Convert.FromBase64String(credentialsBase64);

            using var credentialsStream = new MemoryStream(credentialsJson);

            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromStream(credentialsStream)
            });

            return;
        }

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

    public async Task<User> GetOrCreateUserAsync(string firebaseToken)
    {
        if (string.IsNullOrWhiteSpace(firebaseToken))
        {
            throw new ArgumentException(
                "Firebase token is required.",
                nameof(firebaseToken)
            );
        }

        FirebaseToken decodedToken =
            await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(firebaseToken);

        var firebaseUid = decodedToken.Uid;

        var firebaseEmail = decodedToken.Claims.TryGetValue(
            "email",
            out var emailClaim
        )
            ? emailClaim?.ToString()
            : null;

        if (string.IsNullOrWhiteSpace(firebaseEmail))
        {
            throw new InvalidOperationException(
                "Firebase account does not contain an email address."
            );
        }

        var existingUser = await _userManager.FindByLoginAsync(
            "Firebase",
            firebaseUid
        );

        if (existingUser is not null)
        {
            return existingUser;
        }

        existingUser = await _userManager.FindByEmailAsync(firebaseEmail);

        if (existingUser is not null)
        {
            return existingUser;
        }

        var newUser = new User
        {
            UserName = firebaseEmail,
            Email = firebaseEmail,
            FirebaseUid = firebaseUid,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(newUser);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                ", ",
                result.Errors.Select(error => error.Description)
            );

            throw new InvalidOperationException(
                $"Failed to create local user: {errors}"
            );
        }

        await _userManager.AddLoginAsync(
            newUser,
            new UserLoginInfo(
                "Firebase",
                firebaseUid,
                "Firebase"
            )
        );

        return newUser;
    }
}