using AuthenticationService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();