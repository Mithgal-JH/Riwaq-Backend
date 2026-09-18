using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Team3.Backend.Models;

namespace Team3.Backend.Tests;

public class UserProvisioningServiceTests
{
    [Fact]
    public async Task EnsureLocalUserAsync_ShouldReuseExistingFirebaseUserAndAddLocalClaims()
    {
        using var fixture = new ProvisioningTestContext();
        var user = ServiceTestData.User(Guid.NewGuid());
        user.FirebaseUid = "firebase-existing";
        fixture.DbContext.Users.Add(user);
        await fixture.DbContext.SaveChangesAsync();
        var principal = Principal("firebase-existing", user.Email!);

        await fixture.Service.EnsureLocalUserAsync(principal);

        fixture.DbContext.Users.Should().ContainSingle(existing => existing.Id == user.Id);
        (await fixture.UserManager.GetRolesAsync(user)).Should().Contain("User");
        principal.FindFirstValue(ClaimTypes.NameIdentifier)
            .Should().Be(user.Id.ToString());
        principal.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Should().Contain("User");
    }

    [Fact]
    public async Task EnsureLocalUserAsync_ShouldCreateUserAndDefaultRoleWhenFirebaseUserIsNew()
    {
        using var fixture = new ProvisioningTestContext();
        var principal = Principal("firebase-new", "new@example.com", true);

        await fixture.Service.EnsureLocalUserAsync(principal);

        var user = fixture.DbContext.Users.Single();
        user.FirebaseUid.Should().Be("firebase-new");
        user.Email.Should().Be("new@example.com");
        user.EmailConfirmed.Should().BeTrue();
        (await fixture.UserManager.GetRolesAsync(user)).Should().Equal("User");
        principal.FindFirstValue(ClaimTypes.NameIdentifier)
            .Should().Be(user.Id.ToString());
    }

    [Fact]
    public async Task EnsureLocalUserAsync_ShouldRejectMissingFirebaseUid()
    {
        using var fixture = new ProvisioningTestContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("email", "missing-sub@example.com")],
            "Bearer"));

        var action = () => fixture.Service.EnsureLocalUserAsync(principal);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("The Firebase token does not contain a subject claim.");
    }

    [Fact]
    public async Task EnsureLocalUserAsync_ShouldRejectUnauthenticatedPrincipalAfterIdentityProvisioning()
    {
        using var fixture = new ProvisioningTestContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("sub", "firebase-unauthenticated"),
             new Claim("email", "unauthenticated@example.com")]));

        var action = () => fixture.Service.EnsureLocalUserAsync(principal);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("The authenticated principal has no authenticated identity.");
        fixture.DbContext.Users.Should().ContainSingle(user =>
            user.FirebaseUid == "firebase-unauthenticated");
    }

    [Fact]
    public async Task EnsureLocalUserAsync_ShouldPreserveExistingNonDefaultRole()
    {
        using var fixture = new ProvisioningTestContext();
        var user = ServiceTestData.User(Guid.NewGuid());
        user.FirebaseUid = "firebase-admin";
        fixture.DbContext.Users.Add(user);
        await fixture.DbContext.SaveChangesAsync();
        await fixture.RoleManager.CreateAsync(new IdentityRole<Guid>("Admin"));
        await fixture.UserManager.AddToRoleAsync(user, "Admin");
        var principal = Principal("firebase-admin", user.Email!);

        await fixture.Service.EnsureLocalUserAsync(principal);

        (await fixture.UserManager.GetRolesAsync(user)).Should().Equal("Admin");
        principal.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Should().Equal("Admin");
    }

    private static ClaimsPrincipal Principal(
        string firebaseUid,
        string email,
        bool emailVerified = false)
    {
        var claims = new List<Claim>
        {
            new("sub", firebaseUid),
            new("email", email)
        };
        if (emailVerified)
        {
            claims.Add(new Claim("email_verified", "true"));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
    }
}
