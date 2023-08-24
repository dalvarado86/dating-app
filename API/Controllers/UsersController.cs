using API.Entities;
using API.Entities.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IPhotoService photoService;
        private readonly IMapper mapper;
        private readonly ILogger<UsersController> logger;

        public UsersController(
            IUnitOfWork unitOfWork,
            IPhotoService photoService,
            IMapper mapper,
            ILogger<UsersController> logger)
        {
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            this.photoService = photoService ?? throw new ArgumentNullException(nameof(photoService));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<PaginationList<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            this.logger.LogInformation("Fetching all users.");
            var currentUser = await this.GetCurrentUserAsync();
            userParams.CurrentUsername = currentUser.UserName;

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = currentUser.Gender == "male" ? "male" : "female";
            }

            var members = await this.unitOfWork.UserRepository.GetMembersAsync(userParams);
            this.logger.LogInformation($"{members.Count()} Users have been retrieved.");

            Response.AddPaginationHeader(
                new PaginationHeader(
                    members.CurrentPage,
                    members.PageSize,
                    members.TotalCount,
                    members.TotalPages));

            return Ok(members);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUserbyName(string username)
        {
            this.logger.LogInformation($"Looking for user with username: {username}.");
            var member = await this.unitOfWork.UserRepository.GetMemberByUsernameAsync(username);

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
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return NotFound();
            };

            this.logger.LogInformation($"Updating user: {user.UserName}");
            this.mapper.Map(memberUpdateDto, user);

            if (await this.unitOfWork.Complete())
            {
                this.logger.LogInformation($"User has been updated. Username: {user.UserName}, UserId {user.Id}");
                return NoContent();
            }

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                this.logger.LogInformation($"Cannot get the current user.");
                return NotFound();
            }

            this.logger.LogInformation($"Adding new photo for user: Username: {user.UserName}, FileName: {file.FileName}, Length: {file.Length}");

            var result = await this.photoService.AddPhotoAsync(file);

            if (result.Error != null)
            {
                this.logger.LogError(result.Error.Message);
                return BadRequest(result.Error.Message);
            }

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
            };

            if (user.Photos.Count == 0)
            {
                this.logger.LogInformation($"Setting photo as main.");
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            if (await this.unitOfWork.Complete())
            {
                this.logger.LogInformation($"User has been updated with new photo: Username: {user.UserName}, FileName: {file.FileName}, Length: {file.Length}");
                return CreatedAtAction(
                    nameof(GetUserbyName),
                    new { username = user.UserName },
                    this.mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Failed when adding photo.");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var message = string.Empty;
            var user = await this.GetCurrentUserAsync();

            if (user == null)
            {
                this.logger.LogInformation($"Cannot get the current user.");
                return NotFound();
            };

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null)
            {
                this.logger.LogInformation($"Photo doesn't exist: PhotoId: {photoId}");
                return NotFound();
            }

            if (photo.IsMain)
            {
                message = "This is already your main photo";
                this.logger.LogWarning(message);
                return BadRequest(message);
            }

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

            if (currentMain != null)
            {
                this.logger.LogInformation($"Setting photo as main.");
                currentMain.IsMain = false;
            }

            photo.IsMain = true;

            if (await this.unitOfWork.Complete())
            {
                this.logger.LogInformation($"Main photo has been set for user: Username: {user.UserName}, PhotoId: {photoId}");
                return NoContent();
            }

            message = "Problem setting the main photo";
            this.logger.LogError(message);
            return BadRequest(message);
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var message = string.Empty;
            var user = await this.GetCurrentUserAsync();

            if (user == null)
            {
                this.logger.LogInformation($"Cannot get the current user.");
                return NotFound();
            };

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null)
            {
                this.logger.LogInformation($"Photo doesn't exist: PhotoId: {photoId}");
                return NotFound();
            }

            if (photo.IsMain)
            {
                message = "Cannot delete main photo.";
                this.logger.LogWarning(message);
                return BadRequest(message);
            };

            if (photo.PublicId != null)
            {
                this.logger.LogInformation($"Removing photo from third party service: PhotoId: {photoId}, PublicId: {photo.PublicId}");
                var result = await this.photoService.DeletePhotoAsync(photo.PublicId);

                if (result.Error != null)
                {
                    this.logger.LogError(result.Error.Message);
                    return BadRequest(result.Error.Message);
                }
            }

            this.logger.LogInformation($"Removing photo from user: PhotoId: {photoId}.");
            user.Photos.Remove(photo);

            if (await this.unitOfWork.Complete())
            {
                this.logger.LogInformation($"Photo has been deleted: PhotoId: {photoId}.");
                return Ok();
            };

            message = "Problem deleting photo";
            this.logger.LogError(message);
            return BadRequest(message);
        }

        private async Task<AppUser> GetCurrentUserAsync()
        {
            this.logger.LogInformation($"Trying to get the current username.");
            var username = User.GetUsername();

            this.logger.LogInformation($"Looking for the current user with username: {username}.");
            var user = await this.unitOfWork.UserRepository.GetUserByUsernameAsync(username);

            this.logger.LogInformation($"User has been retrieved. Username: {username}, UserId: {user.Id}");
            return user;
        }
    }
}