using Api.Dtos;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// Login is public — no token needed here, this is where you get one.
[ApiController]
[Route("auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    // Username/password from config (Auth:Username / Auth:Password).
    // Returns a Bearer token the frontend uses for /reminders calls.
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var response = authService.Login(request);
        if (response is null)
        {
            return Unauthorized();
        }

        return Ok(response);
    }
}
