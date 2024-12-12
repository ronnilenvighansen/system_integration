using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationService.Services
{
    public class JwtTokenService
    {
        private readonly string _securityKey;

        public JwtTokenService(IConfiguration configuration)
        {
            _securityKey = configuration["jwt:secret"] 
                           ?? throw new InvalidOperationException("JWT secret is missing from configuration.");
        }

        public AuthenticationToken CreateUserToken()
        {
            return CreateToken(new List<Claim>
            {
                new Claim("scope", "user.all")
            });
        }

        public AuthenticationToken CreatePostToken()
        {
            return CreateToken(new List<Claim>
            {
                new Claim("scope", "post.all")
            });
        }

        private AuthenticationToken CreateToken(List<Claim> claims)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityKey));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var tokenOptions = new JwtSecurityToken(
                claims: claims,
                signingCredentials: signingCredentials,
                expires: DateTime.UtcNow.AddHours(1)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return new AuthenticationToken { Value = tokenString };
        }
    }

    public class AuthenticationToken
    {
        public string Value { get; set; }
    }
}