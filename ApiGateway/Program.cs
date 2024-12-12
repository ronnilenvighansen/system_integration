using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

builder.Configuration.AddJsonFile("appsettings.json", true, true);
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

var jwtSecret = builder.Configuration["jwt:secret"];

if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new Exception("JWT secret is not configured properly.");
}

builder.Services.AddAuthentication(
    options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }
    ).AddJwtBearer(
    JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };

    }
);    

builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseOcelot().Wait();

app.UseAuthorization();

app.MapControllers();

app.Run();