using API.Entities;
using API.Entities.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUserRepository userRepository;
        private readonly ILikesRepository likesRepository;
        
        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.likesRepository = likesRepository ?? throw new ArgumentNullException(nameof(likesRepository));
        }

         [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await this.userRepository.GetUserByUsernameAsync(username);
            var sourceUser = await this.likesRepository.GetUserWithLikesAsync(sourceUserId);

            if (likedUser == null) 
            {
                return NotFound();
            }

            if (sourceUser.UserName == username) 
            {
                return BadRequest("Users cannot liked themselves.");
            }

            var userLike = await this.likesRepository.GetUserLikeAsync(sourceUserId, likedUser.Id);

            if (userLike != null) 
            {
                return BadRequest("User is already liked.");
            }

            userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = likedUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);

            if (await this.userRepository.SaveAllAsync()) 
            {
                return Ok();
            }

            return BadRequest("Failed to add like for user.");
        }

        [HttpGet]
        public async Task<ActionResult<PaginationList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await this.likesRepository
                .GetUserLikesAsync(likesParams);

            Response.AddPaginationHeader(
                new PaginationHeader(
                    users.CurrentPage,
                    users.PageSize, 
                    users.TotalCount, 
                    users.TotalPages));

            return Ok(users);
        }
    }
}