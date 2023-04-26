using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly DataContext context;
        private readonly ILogger<UsersController> logger;
        
        public UsersController(DataContext context, ILogger<UsersController> logger)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
        {
            this.logger.LogInformation("Fetching all users.");
            var users = await this.context.Users.ToListAsync();
            this.logger.LogInformation($"{users.Count} Users have been retrieved.");
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetUsers(int id)
        {
            this.logger.LogInformation($"Looking for user with id {id}.");
            var user = await this.context.Users.FindAsync(id);
            
            if (user is null)
            {
                 this.logger.LogInformation($"User with id {id} does not exist.");
                return NotFound();
            }

            this.logger.LogInformation($"User has been retrieved: id: {user.Id}, username: {user.Username}.");
            return Ok(user);
        }
    }
}