using API.Entities;
using API.Entities.DTOs;
using API.Helpers;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        Task<UserLike> GetUserLikeAsync(int sourceUserId, int likedUserId);
        Task<AppUser> GetUserWithLikesAsync(int userId);
        Task<PaginationList<LikeDto>> GetUserLikesAsync(LikesParams likesParams);
    }
}