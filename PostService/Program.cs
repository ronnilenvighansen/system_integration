using Microsoft.EntityFrameworkCore;
using EasyNetQ;
using Shared.Services;
using PostService.Controllers;
using Polly;
using PostService.Services;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);

var baseAddress = builder.Configuration["UserService:BaseAddress"];

var retryPolicy = Policy
    .Handle<HttpRequestException>() // For general HTTP request failures
    .Or<SocketException>()          // For DNS-related issues
    .OrInner<SocketException>()     // Handle inner exceptions for wrapped errors
    .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode) // Retry for non-successful responses
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

builder.Services.AddHttpClient<IDValidationService>(client =>
{
    client.BaseAddress = new Uri(baseAddress);
})
.AddHttpMessageHandler<PolicyHandler>();

builder.Services.AddSingleton<PolicyHandler>();

builder.Services.AddSingleton<IBus>(sp => CreateBus());

var sqlitePath = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
    ? "/app/appdata/PostDb.sqlite"  // Path for Docker
    : "PostDb.sqlite";              // Path for local development

builder.Services.AddDbContext<PostDbContext>(options =>
    options.UseSqlite($"Data Source={sqlitePath}"));

builder.Services.AddScoped<PostDbContext>();

builder.Services.AddScoped<UserCreatedSubscriber>();

builder.Services.AddHttpClient<PostController>();

builder.Services.AddControllers();

builder.Services.AddScoped<IMessagePublisher, MessagePublisher>();

builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>(client =>
{
    client.BaseAddress = new Uri(baseAddress);
});

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

static IBus CreateBus()
{
    var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
    return RabbitHutch.CreateBus($"host={host};username=guest;password=guest");
}
