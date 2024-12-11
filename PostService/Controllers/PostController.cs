using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostService.Services;
using Shared.Services;

namespace PostService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PostController : ControllerBase
{
    private readonly IIDValidationService _idValidationService;
    private readonly PostDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IMessagePublisher _messagePublisher;

    public PostController(IIDValidationService idValidationService, PostDbContext context, IHttpClientFactory httpClientFactory, IMessagePublisher messagePublisher)
    {
        _idValidationService = idValidationService;
        _context = context;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("http://userservice");
        _messagePublisher = messagePublisher;
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
            var isValidID = await _idValidationService.ValidateIDAsync(post.UserId);
            if (!isValidID)
            {
                return BadRequest("Invalid ID.");
            }

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

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost(Guid id, [FromBody] Post updatedPost)
    {
        if (id != updatedPost.Id)
        {
            return BadRequest("Post ID mismatch.");
        }

        var existingPost = await _context.Posts.FindAsync(id);
        if (existingPost == null)
        {
            return NotFound("Post not found.");
        }

        try
        {
            // Update the existing post with new values
            existingPost.Content = updatedPost.Content;
            existingPost.CreatedAt = DateTime.UtcNow;

            // Save changes to the database
            _context.Entry(existingPost).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent(); // Return 204 No Content to indicate success
        }
        catch (DbUpdateConcurrencyException)
        {
            // Handle concurrency issues if needed
            if (!_context.Posts.Any(p => p.Id == id))
            {
                return NotFound("Post no longer exists.");
            }

            throw;
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
