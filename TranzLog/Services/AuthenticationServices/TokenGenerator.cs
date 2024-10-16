using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TranzLog.Interfaces;
using TranzLog.Models;

namespace TranzLog.Services.AuthenticationServices
{
    public class TokenGenerator : ITokenGenerator
    {
        private readonly IConfiguration config;
        public TokenGenerator(IConfiguration config)
        {
            this.config = config;
        }
        public string GetToken(User user)
        {
            var key = config["Jwt:Key"];
            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("В конфигурации отсутствует ключ JWT.");
            }
            var issuer = config["Jwt:Issuer"];
            var audience = config["Jwt:Audience"];
            if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
            {
                throw new InvalidOperationException("В конфигурации JWT отсутствует Issuer или Audience.");
            }
            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };
            var token = new JwtSecurityToken(config["Jwt:Issuer"],
                config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
