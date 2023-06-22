using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.Entities.Requests;
using API.Entities.Responses;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    // TODO: Adds fluent validations
    public class AccountsController : BaseApiController
    {
        private readonly ILogger<AccountsController> logger;
        private readonly ITokenService tokenService;
        private readonly DataContext context;

        public AccountsController(
            DataContext context, 
            ITokenService tokenService,
            ILogger<AccountsController> logger)
        {
            this.tokenService = tokenService ?? throw new ArgumentNullException(nameof(context));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserResponse>> Register(RegisterRequest register)
        {
            this.logger.LogInformation($"Validating username provided: '{register.Username}'.");

            if (await UserExists(register.Username))
            {
                return BadRequest($"Username '{register.Username}' is already taken.");
            }

            using var hmac = new HMACSHA512();
            var user = new AppUser 
            {
                Username = register.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(register.Password)),
                PasswordSalt = hmac.Key,
            };

            this.context.Users.Add(user);
            await this.context.SaveChangesAsync();
            this.logger.LogInformation($"User '{user.Username}' has been created.");

            var userDto = new UserResponse 
            (
                Username: user.Username,
                Token: this.tokenService.CreateToken(user),
                PhotoUrl: string.Empty
            );

            return Ok(userDto);
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserResponse>> Login(LoginRequest login)
        {
            this.logger.LogInformation($"Looking for provided username: '{login.Username}'.");
            var user = await this.context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.Username == login.Username);
            
            const string unauthorizedMessage = "The username or password are invalid.";

            if (user is null)
            {
                return Unauthorized(unauthorizedMessage);
            }

            this.logger.LogInformation("Validating password.");
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(login.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                {
                    return Unauthorized(unauthorizedMessage);
                }
            }

            this.logger.LogInformation($"Credentials for user '{user.Username}' has been validated.");
            
            var userDto = new UserResponse 
            (
                Username: user.Username,
                Token: this.tokenService.CreateToken(user),
                PhotoUrl: user.Photos.FirstOrDefault(x => x.IsMain)?.Url
            );

            return Ok(userDto);
        }

        private async Task<bool> UserExists(string username) 
        {
            return await this.context.Users.AnyAsync(x => x.Username == username.ToLower());
        }
    }
}