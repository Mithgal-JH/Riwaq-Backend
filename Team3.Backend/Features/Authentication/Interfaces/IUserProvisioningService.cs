using System.Security.Claims;

namespace Team3.Backend.Features.Authentication.Interfaces;

public interface IUserProvisioningService
{
    Task EnsureLocalUserAsync(ClaimsPrincipal principal);
}
