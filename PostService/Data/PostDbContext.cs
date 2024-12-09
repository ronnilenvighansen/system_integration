using Microsoft.EntityFrameworkCore;
using Shared.Models;

public class PostDbContext : DbContext
{
    public PostDbContext(DbContextOptions<PostDbContext> options) : base(options) { }

    public DbSet<Post> Posts { get; set; }
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<UserCreatedMessage> UserCreatedMessages { get; set; }
    public DbSet<ProcessingSuccessMessage> ProcessingSuccessMessages { get; set; }

}
