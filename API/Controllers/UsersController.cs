using System.Security.Claims;
using API.Entities.DTOs;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository repository;
         private readonly IMapper mapper;
        private readonly ILogger<UsersController> logger;
        
        public UsersController(IUserRepository repository, IMapper mapper, ILogger<UsersController> logger)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            this.logger.LogInformation("Fetching all users.");
            var members = await this.repository.GetMembersAsync();
            this.logger.LogInformation($"{members.Count()} Users have been retrieved.");
            return Ok(members);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUserbyName(string username)
        {
            this.logger.LogInformation($"Looking for user with username: {username}.");
            var member = await this.repository.GetMemberByUsernameAsync(username);
            
            if (member is null)
            {
                 this.logger.LogInformation($"User with username {username} does not exist.");
                return NotFound();
            }

            this.logger.LogInformation($"User has been retrieved: username: {member.Username}.");
            return Ok(member);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUserAsync(MemberUpdateDto memberUpdateDto)
        {
            this.logger.LogInformation($"Trying to get the current username.");
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            this.logger.LogInformation($"Looking for the current user with username: {username}.");
            var user = await this.repository.GetUserByUsernameAsync(username);

            if (user == null) 
            {
                return NotFound();
            };

            this.mapper.Map(memberUpdateDto, user);

            if (await this.repository.SaveAllAsync())
            {
                this.logger.LogInformation($"User has been updated: username: {username}.");
                return NoContent();
            }

            return BadRequest("Failed to update user");
        }
    }
}