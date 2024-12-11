using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EasyNetQ;
using Shared.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IBus>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>(); // Get the configuration
    return CreateBus(configuration); // Pass the configuration to CreateBus
});

string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

builder.Services.AddDbContext<UserDbContext>(options =>
{
    string connectionString = environment switch
    {
        "Development" => builder.Configuration.GetConnectionString("DevelopmentConnection"),
        "Docker" => builder.Configuration.GetConnectionString("DockerConnection"),
        "Kubernetes" => builder.Configuration.GetConnectionString("KubernetesConnection"),
        _ => throw new Exception("No valid environment detected")
    };

    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 0)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()
    );
});

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<UserDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();

builder.Services.AddScoped<IMessagePublisher, MessagePublisher>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();

    try
    {
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
            Console.WriteLine("Migrations applied successfully.");
        }
        else
        {
            Console.WriteLine("No pending migrations.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while applying migrations: {ex.Message}");
    }
}


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

static IBus CreateBus(IConfiguration configuration)
{
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

    var rabbitMqConnection = environment switch
    {
        "Development" => configuration["RabbitMQ:DevelopmentConnection"],
        "Docker" => configuration["RabbitMQ:DockerConnection"],
        "Kubernetes" => configuration["RabbitMQ:KubernetesConnection"],
        _ => throw new Exception("No valid environment detected")
    };

    Console.WriteLine($"Using RabbitMQ Connection String: {rabbitMqConnection}");

    return RabbitHutch.CreateBus(rabbitMqConnection);
}