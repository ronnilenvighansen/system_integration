public class UserCreatedSubscriberBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public UserCreatedSubscriberBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var userCreatedSubscriber = scope.ServiceProvider.GetRequiredService<UserCreatedSubscriber>();

            // Start subscription handling
            userCreatedSubscriber.Start();

            // Keep the service running until stopped
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Expected exception on service cancellation
            Console.WriteLine("UserCreatedSubscriberBackgroundService is stopping.");
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            Console.WriteLine($"Unexpected error in UserCreatedSubscriberBackgroundService: {ex.Message}");
        }
    }
}
