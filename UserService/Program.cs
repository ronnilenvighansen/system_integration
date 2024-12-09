using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EasyNetQ;
using Shared.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IBus>(sp => CreateBus());

var sqlitePath = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
    ? "/app/appdata/UserDb.sqlite"  // Path for Docker
    : "UserDb.sqlite";              // Path for local development

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlite($"Data Source={sqlitePath}"));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<UserDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();

builder.Services.AddScoped<IMessagePublisher, MessagePublisher>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
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