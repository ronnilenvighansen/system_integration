using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class PostController : ControllerBase
{
    private readonly PostDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IUserServiceClient _userServiceClient;

    public PostController(PostDbContext context, IHttpClientFactory httpClientFactory, IUserServiceClient userServiceClient)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("http://userservice");
        _userServiceClient = userServiceClient;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
    {
        return await _context.Posts.ToListAsync();
    }

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok("Service is running");
    }

    [HttpPost]
    public async Task<ActionResult<Post>> CreatePost([FromBody] Post post)
    {   
        try
        {
            var user = await _userServiceClient.GetUserByIdAsync(post.UserId);
            if (user == null || string.IsNullOrEmpty(user.Id))
            {
                return BadRequest("Invalid or non-existent user.");
            }

            post.UserId = user.Id;
            post.CreatedAt = DateTime.UtcNow;

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPosts), new { id = post.Id }, post);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching user: {ex.Message}");
            return StatusCode(500, "User service communication error.");
        }
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
