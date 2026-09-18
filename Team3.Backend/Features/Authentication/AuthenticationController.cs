using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Team3.Backend.Features.Authentication.Dtos;
using Team3.Backend.Features.Authentication.Interfaces;

namespace Team3.Backend.Features.Authentication;

[ApiController]
[Route("api/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationController(
        IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("firebase-login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> FirebaseLogin(
        [FromBody] FirebaseLoginRequest request)
    {
        try
        {
            var response = await _authenticationService
                .LoginWithFirebaseAsync(request);

            return Ok(response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
        catch (FirebaseAuthException)
        {
            return Unauthorized(new
            {
                message = "Invalid or expired Firebase ID token."
            });
        }
    }
}