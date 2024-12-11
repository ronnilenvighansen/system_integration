using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
public class PostDbContextFactory : IDesignTimeDbContextFactory<PostDbContext>
{
    public PostDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = environment switch
        {
            "Development" => configuration.GetConnectionString("DevelopmentConnection"),
            "Docker" => configuration.GetConnectionString("DockerConnection"),
            "Kubernetes" => configuration.GetConnectionString("KubernetesConnection"),
            _ => throw new Exception("No valid environment detected")
        };

        var optionsBuilder = new DbContextOptionsBuilder<PostDbContext>();
        optionsBuilder.UseMySql(
            connectionString,
            new MySqlServerVersion(new Version(8, 0, 0)),
            mySqlOptions => mySqlOptions.EnableRetryOnFailure()
        );

        return new PostDbContext(optionsBuilder.Options);
    }
}
