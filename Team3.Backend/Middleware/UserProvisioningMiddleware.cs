using Team3.Backend.Features.Authentication.Interfaces;

namespace Team3.Backend.Middleware;

public class UserProvisioningMiddleware
{
    private readonly RequestDelegate _next;

    public UserProvisioningMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IUserProvisioningService userProvisioningService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            await userProvisioningService.EnsureLocalUserAsync(context.User);
        }

        await _next(context);
    }
}
