using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EasyNetQ;
using UserService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IBus>(sp => CreateBus());

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlite("Data Source=UserDb.sqlite"));

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