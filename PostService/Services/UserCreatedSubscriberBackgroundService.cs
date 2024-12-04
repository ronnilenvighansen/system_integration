public class UserCreatedSubscriberBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public UserCreatedSubscriberBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var userCreatedSubscriber = scope.ServiceProvider.GetRequiredService<UserCreatedSubscriber>();
        userCreatedSubscriber.Start();

        await Task.CompletedTask;
    }
}

