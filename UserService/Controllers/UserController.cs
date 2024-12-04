using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostService.Models;
using UserService.Services;

namespace PostService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IMessagePublisher _messagePublisher;

    public UserController(UserManager<User> userManager, IMessagePublisher messagePublisher)
    {
        _userManager = userManager;
        _messagePublisher = messagePublisher;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _userManager.Users.ToListAsync();
    }

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok("Service is running");
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserByIdAlt(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpGet("byid/{userId}")]
    public async Task<IActionResult> GetUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(new UserIdDto { Id = user.Id });
    }

    [HttpGet("byusername/{username}")]
    public async Task<IActionResult> GetUserId(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        return Ok(new { Id = user.Id});
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
    {
        var user = new User
        {
            UserName = registerModel.UserName,
            Email = registerModel.Email,
            FullName = registerModel.FullName
        };

        var result = await _userManager.CreateAsync(user, registerModel.Password);

        if (result.Succeeded)
        {
            var userCreatedMessage = new UserCreatedMessage
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };
            await _messagePublisher.PublishUserCreatedMessage(userCreatedMessage);

            return Ok(user);
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
    {
        var user = await _userManager.FindByNameAsync(loginModel.UserName);
        if (user != null && await _userManager.CheckPasswordAsync(user, loginModel.Password))
        {
            return Ok();
        }

        return Unauthorized();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserModel updateRequest)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        if (!string.IsNullOrEmpty(updateRequest.FullName))
        {
            user.FullName = updateRequest.FullName;
        }
        if (!string.IsNullOrEmpty(updateRequest.Email))
        {
            user.Email = updateRequest.Email;
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return NoContent();
    }

    public class UserIdDto
    {
        public string Id { get; set; }
    }
}
