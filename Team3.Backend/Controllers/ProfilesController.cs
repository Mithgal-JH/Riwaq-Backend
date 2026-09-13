using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProfilesController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Profiles API is working");
    }
}