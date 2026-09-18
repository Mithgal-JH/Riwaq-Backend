using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Team3.Backend.Features.Authentication;

namespace Team3.Backend.Tests;

public class CurrentUserServiceTests
{
    [Fact]
    public void UserId_ShouldResolveFromNameIdentifierClaim()
    {
        var userId = Guid.NewGuid();
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
                "Bearer"))
        };

        var service = new CurrentUserService(new HttpContextAccessor
        {
            HttpContext = context
        });

        service.UserId.Should().Be(userId);
    }

    [Fact]
    public void UserId_ShouldBeNullWhenClaimIsMissing()
    {
        var service = CreateService(new ClaimsPrincipal(
            new ClaimsIdentity([], "Bearer")));

        service.UserId.Should().BeNull();
    }

    [Fact]
    public void UserId_ShouldBeNullWhenClaimIsNotAGuid()
    {
        var service = CreateService(new ClaimsPrincipal(
            new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "not-a-guid")],
                "Bearer")));

        service.UserId.Should().BeNull();
    }

    [Fact]
    public void FirebaseUid_ShouldResolveFromSubjectClaim()
    {
        var service = CreateService(new ClaimsPrincipal(
            new ClaimsIdentity([new Claim("sub", "firebase-123")], "Bearer")));

        service.FirebaseUid.Should().Be("firebase-123");
    }

    private static CurrentUserService CreateService(ClaimsPrincipal principal)
    {
        return new CurrentUserService(new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = principal }
        });
    }
}
