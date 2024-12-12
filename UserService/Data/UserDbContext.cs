using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserService.Models;

public class UserDbContext : IdentityDbContext<User>
{
    public DbSet<DomainEvent> DomainEvents { get; set; }
    public DbSet<UserReadModel> UserReadModels { get; set; }

    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<DomainEvent>().HasKey(e => e.Id);
    }
}
