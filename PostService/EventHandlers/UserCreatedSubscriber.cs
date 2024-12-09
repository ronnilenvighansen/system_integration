using EasyNetQ;
using Shared.Models;

public class UserCreatedSubscriber
{
    private readonly IBus _bus;
    private readonly IServiceProvider _serviceProvider;

    public UserCreatedSubscriber(IBus bus, IServiceProvider serviceProvider)
    {
        _bus = bus;
        _serviceProvider = serviceProvider;
    }

    public void Start()
    {
        // Subscribe to the event, make sure scope creation happens here
        _bus.PubSub.Subscribe<UserCreatedMessage>("post_service_subscription", async message =>
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<PostDbContext>();

                try
                {
                    Console.WriteLine($"User created: {message.UserId}, {message.UserName}, {message.Email}");

                    // Save the message and user to the database
                    context.UserCreatedMessages.Add(message);
                    await context.SaveChangesAsync();

                    var user = new UserEntity
                    {
                        Id = message.UserId,
                        UserName = message.UserName,
                        Email = message.Email
                    };

                    context.Users.Add(user);
                    await context.SaveChangesAsync();

                    var successMessage = new ProcessingSuccessMessage
                    {
                        UserId = message.UserId,
                        UserName = message.UserName,
                        ProcessedAt = DateTime.UtcNow,
                        StatusMessage = "User created and saved successfully in PostService."
                    };

                    context.ProcessingSuccessMessages.Add(successMessage);
                    await context.SaveChangesAsync();

                    // Publish success message to the bus
                    await _bus.PubSub.PublishAsync(successMessage);

                    Console.WriteLine($"{message.UserId}, {message.UserName}, {successMessage.ProcessedAt}, {successMessage.StatusMessage}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                }
            }
        });
    }
}
