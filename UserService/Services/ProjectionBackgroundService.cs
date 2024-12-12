public class ProjectionBackgroundService : BackgroundService
{
    private readonly UserReadModelProjection _projection;

    public ProjectionBackgroundService(UserReadModelProjection projection)
    {
        _projection = projection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _projection.Project();
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
