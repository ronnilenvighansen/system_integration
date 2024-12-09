using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using Shared.Services;

namespace UserService.Controllers;

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

    [HttpGet("validate-id/{userId}")]
    public async Task<IActionResult> ValidateID(string userId)
    {
        Console.WriteLine($"Validating ID: {userId}");

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            Console.WriteLine($"ID {userId} not found.");
            return NotFound("ID not found.");
        }

        Console.WriteLine($"ID {userId} is valid.");
        return Ok("ID is valid.");
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return NoContent();
    }

}
