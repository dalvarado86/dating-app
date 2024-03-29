using API.Entities;
using API.Entities.DTOs;
using API.Helpers;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);
        Task<IEnumerable<AppUser>> GetUsersAsync();
        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByUsernameAsync(string username);
        Task<MemberDto> GetMemberAsync(string username, bool isCurrentUser);
        Task<PaginationList<MemberDto>> GetMembersAsync(UserParams userParams);
        Task<MemberDto> GetMemberByUsernameAsync(string username);
        Task<AppUser> GetUserByPhotoIdAsync(int photoId);
    }
}