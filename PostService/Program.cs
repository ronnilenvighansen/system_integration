using Microsoft.EntityFrameworkCore;
using EasyNetQ;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IBus>(sp => CreateBus());

builder.Services.AddDbContext<PostDbContext>(options =>
    options.UseSqlite("Data Source=PostDb.sqlite"));

builder.Services.AddScoped<UserCreatedSubscriber>();

builder.Services.AddHttpClient<PostController>();

builder.Services.AddControllers();

builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>(client =>
{
    var userServiceUrl = Environment.GetEnvironmentVariable("USER_SERVICE_URL") ?? "http://localhost:5006/api/user/";
    client.BaseAddress = new Uri(userServiceUrl);
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
app.Run();

static IBus CreateBus()
{
    var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
    return RabbitHutch.CreateBus($"host={host};username=guest;password=guest");
}
