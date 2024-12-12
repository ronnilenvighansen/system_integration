using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using Shared.Services;
using UserService.Commands;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IMessagePublisher _messagePublisher;
    private readonly UserCommandHandler _commandHandler;


    public UserController(UserManager<User> userManager, IMessagePublisher messagePublisher, UserCommandHandler commandHandler)
    {
        _userManager = userManager;
        _messagePublisher = messagePublisher;
        _commandHandler = commandHandler;

        
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserReadModel>>> GetUsers()
    {
        var users = await _userManager.Users.Select(u => new UserReadModel
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            FullName = u.FullName
        }).ToListAsync();

        return Ok(users);
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

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
    {
        var user = new User
        {
            UserName = registerModel.UserName,
            Email = registerModel.Email,
            FullName = registerModel.FullName
        };
        var result = await _userManager.CreateAsync(user, registerModel.Password);
        var command = new CreateUserCommand
        {
            UserName = registerModel.UserName,
            Email = registerModel.Email,
            FullName = registerModel.FullName
        };
        if (result.Succeeded)
        {
            var userCreatedMessage = new UserCreatedMessage
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };
            await _messagePublisher.PublishUserCreatedMessage(userCreatedMessage);
            await _commandHandler.Handle(command);

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
    public async Task<IActionResult> DeleteUser(string id, DeleteUserCommand command)
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

        var userDeletedMessage = new UserDeletedMessage
        {
            UserId = user.Id,
            UserName = user.UserName
        };
        await _messagePublisher.PublishUserDeletedMessage(userDeletedMessage);
        await _commandHandler.Handle(command);
        return NoContent();
    }
}
