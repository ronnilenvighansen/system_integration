using AuthenticationService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly JwtTokenService _jwtTokenService;

    public AuthenticationController(JwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("user")]
    public IActionResult LoginUser()
    {
        var token = _jwtTokenService.CreateUserToken();
        return Ok(token);
    }

    [HttpPost("post")]
    public IActionResult LoginPost()
    {
        var token = _jwtTokenService.CreatePostToken();
        return Ok(token);
    }
}
