using Microsoft.EntityFrameworkCore;
using EasyNetQ;
using Shared.Services;
using PostService.Controllers;
using Polly;
using PostService.Services;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IIDValidationService, IDValidationService>();

builder.Services.AddTransient<PolicyHandler>();

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

builder.Services.AddDbContext<PostDbContext>(options =>
{
    var connectionString = environment switch
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

builder.Services.AddHttpClient<IIDValidationService, IDValidationService>((provider, client) =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();

    var baseAddress = environment switch
    {
        "Development" => configuration["BaseAddresses:DevelopmentBaseAddress"],
        "Docker" => configuration["BaseAddresses:DockerBaseAddress"],
        "Kubernetes" => configuration["BaseAddresses:KubernetesBaseAddress"],
        _ => throw new Exception("No valid environment detected")
    };

    Console.WriteLine($"Configuring HttpClient in AddHttpClient with BaseAddress: {baseAddress}");
    client.BaseAddress = new Uri(baseAddress);
}).AddHttpMessageHandler<PolicyHandler>();


var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .Or<SocketException>()
    .OrInner<SocketException>()
    .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
    .RetryAsync(3, onRetry: (outcome, retryNumber, context) =>
    {
        if (outcome.Exception != null)
        {
            Console.WriteLine($"Retry {retryNumber} due to exception: {outcome.Exception.Message}");
        }
        else
        {
            Console.WriteLine($"Retry {retryNumber} due to response: {outcome.Result?.StatusCode}");
        }
    });

var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
var combinedPolicy = Policy.WrapAsync(retryPolicy, timeoutPolicy);

builder.Services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(combinedPolicy);

builder.Services.AddSingleton<IBus>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>(); // Get the configuration
    return CreateBus(configuration); // Pass the configuration to CreateBus
});

builder.Services.AddScoped<PostDbContext>();

builder.Services.AddScoped<UserCreatedSubscriber>();

builder.Services.AddHttpClient<PostController>();

builder.Services.AddControllers();

builder.Services.AddScoped<IMessagePublisher, MessagePublisher>();

builder.Services.AddHostedService<UserCreatedSubscriberBackgroundService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PostDbContext>();
    dbContext.Database.Migrate();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStopping.Register(() =>
{
    var bus = app.Services.GetRequiredService<IBus>();
    bus.Dispose();
});

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
