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

            userCreatedSubscriber.Start();

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("UserCreatedSubscriberBackgroundService is stopping.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error in UserCreatedSubscriberBackgroundService: {ex.Message}");
        }
    }
}
