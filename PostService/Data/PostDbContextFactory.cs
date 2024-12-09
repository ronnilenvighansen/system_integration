using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class PostDbContextFactory : IDesignTimeDbContextFactory<PostDbContext>
{
    public PostDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<PostDbContext>();
        optionsBuilder.UseSqlite(configuration.GetConnectionString("PostConnection"));

        return new PostDbContext(optionsBuilder.Options);
    }
}
