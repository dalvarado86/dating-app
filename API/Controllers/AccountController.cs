using AutoMapper;
using API.Entities.Requests;
using API.Entities.Responses;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountsController : BaseApiController
    {
        private readonly ILogger<AccountsController> logger;
        private readonly ITokenService tokenService;
        private readonly UserManager<AppUser> userManager;
        private readonly IMapper mapper;

        public AccountsController(
            UserManager<AppUser> userManager,
            ITokenService tokenService,
            IMapper mapper,
            ILogger<AccountsController> logger)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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

            var user = mapper.Map<AppUser>(register);

            var result = await this.userManager
               .CreateAsync(user, register.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var roleResult = await this.userManager.AddToRoleAsync(user, "Member");

            if (!roleResult.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            this.logger.LogInformation($"User '{user.UserName}' has been created.");

            var userDto = new UserResponse
            (
                Username: user.UserName,
                Token: await this.tokenService.CreateToken(user),
                KnownAs: user.KnownAs,
                Gender: user.Gender,
                PhotoUrl: string.Empty
            );

            return Ok(userDto);
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserResponse>> Login(LoginRequest login)
        {
            this.logger.LogInformation($"Looking for provided username: '{login.Username}'.");
            var user = await this.userManager.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == login.Username);

            const string unauthorizedMessage = "The username or password are invalid.";

            if (user is null)
            {
                return Unauthorized(unauthorizedMessage);
            }

            this.logger.LogInformation("Validating password.");
            var result = await this.userManager.CheckPasswordAsync(user, login.Password);

            if (!result)
            {
                return Unauthorized();
            }

            this.logger.LogInformation($"Credentials for user '{user.UserName}' has been validated.");

            var userDto = new UserResponse
            (
                Username: user.UserName,
                Token: await this.tokenService.CreateToken(user),
                KnownAs: user.KnownAs,
                Gender: user.Gender,
                PhotoUrl: user.Photos.FirstOrDefault(x => x.IsMain)?.Url
            );

            return Ok(userDto);
        }

        private async Task<bool> UserExists(string username)
        {
            return await this.userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}