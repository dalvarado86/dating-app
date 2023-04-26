using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public class TokenService : ITokenService
    {
        private const string KeyName = "TokenKey";
        private const int ExpiringTimeInDays = 7;
        private readonly SymmetricSecurityKey symmetricSecurityKey;
        private readonly ILogger<TokenService> logger;

        public TokenService(IConfiguration config, ILogger<TokenService> logger)
        {
            ArgumentNullException.ThrowIfNull(config);
            ArgumentException.ThrowIfNullOrEmpty(config[KeyName]);

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.symmetricSecurityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(config[KeyName]));
        }

        public string CreateToken(AppUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            this.logger.LogInformation($"Generating token for user '{user.Username}'.");

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Username),
            };

            var credentials = new SigningCredentials(this.symmetricSecurityKey, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(ExpiringTimeInDays),
                SigningCredentials = credentials,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            var writedToken = tokenHandler.WriteToken(token);
            
            this.logger.LogInformation($"Token for user '{user.Username}' has been generated.");
            return writedToken;
        }
    }
}