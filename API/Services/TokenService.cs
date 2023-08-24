using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public class TokenService : ITokenService
    {
        private const string KeyName = "TokenKey";
        private const int ExpiringTimeInDays = 7;
        private readonly UserManager<AppUser> userManager;
        private readonly SymmetricSecurityKey symmetricSecurityKey;
        private readonly ILogger<TokenService> logger;

        public TokenService(
            IConfiguration config,
            UserManager<AppUser> userManager,
            ILogger<TokenService> logger)
        {
            ArgumentNullException.ThrowIfNull(config);
            ArgumentException.ThrowIfNullOrEmpty(config[KeyName]);

            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.symmetricSecurityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(config[KeyName]));
        }

        public async Task<string> CreateToken(AppUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            this.logger.LogInformation($"Generating token for user '{user.UserName}'.");

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            };

            var roles = await this.userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

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
            
            this.logger.LogInformation($"Token for user '{user.UserName}' has been generated.");
            return writedToken;
        }
    }
}