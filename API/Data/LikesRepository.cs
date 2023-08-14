using API.Entities;
using API.Entities.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext dbContext;

        public LikesRepository(DataContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<UserLike> GetUserLikeAsync(int sourceUserId, int likedUserId)
        {
            return await this.dbContext.Likes
                .FindAsync(sourceUserId, likedUserId);
        }

        public async Task<PaginationList<LikeDto>> GetUserLikesAsync(LikesParams likesParams)
        {
            var users = this.dbContext.Users
                .OrderBy(u => u.Username)
                .AsQueryable();
            
            var likes = this.dbContext.Likes
                .AsQueryable();

            if (string.Equals(likesParams.Predicate, "liked"))
            {
                likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
                users = likes.Select(like => like.TargetUser);
            }

            if (string.Equals(likesParams.Predicate, "likedBy"))
            {
                likes = likes.Where(like => like.TargetUserId == likesParams.UserId);
                users = likes.Select(like => like.SourceUser);
            }

            var likedUsers = users.Select(user => new LikeDto {
                UserName = user.Username,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalcuateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
                City = user.City,
                Id = user.Id
            });

            return await PaginationList<LikeDto>
                .CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLikesAsync(int userId)
        {
             return await this.dbContext.Users
                .Include(x => x.LikedUsers)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}