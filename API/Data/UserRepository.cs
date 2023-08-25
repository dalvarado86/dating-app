using API.Entities;
using API.Entities.DTOs;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;

        public UserRepository(DataContext context, IMapper mapper)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<MemberDto> GetMemberAsync(string username, bool isCurrentUser)
        {
            var query = this.context.Users
                .Where(x => x.UserName == username)
                .ProjectTo<MemberDto>(this.mapper.ConfigurationProvider)
                .AsQueryable();

            if (isCurrentUser) 
            {
                query = query.IgnoreQueryFilters();
            }
            
            return await query.FirstOrDefaultAsync();
        }


        public async Task<MemberDto> GetMemberByUsernameAsync(string username)
        {
            ArgumentException.ThrowIfNullOrEmpty(username, nameof(username));

            return await this.context.Users
                .Where(x => x.UserName == username)
                .ProjectTo<MemberDto>(this.mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        public async Task<PaginationList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            ArgumentNullException.ThrowIfNull(userParams, nameof(userParams));

            var query = this.context.Users.AsQueryable();

            query = query.Where(u => u.UserName != userParams.CurrentUsername);
            query = query.Where(u => u.Gender == userParams.Gender);

            var minDateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
            var maxDateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

            query = query.Where(u => u.DateOfBirth >= minDateOfBirth
                                && u.DateOfBirth <= maxDateOfBirth);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(u => u.LastActive)
            };

            return await PaginationList<MemberDto>.CreateAsync(
                query.AsNoTracking().ProjectTo<MemberDto>(this.mapper.ConfigurationProvider),
                userParams.PageNumber,
                userParams.PageSize
            );
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await this.context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByPhotoIdAsync(int photoId)
        {
            return await this.context.Users
                .Include(p => p.Photos)
                .IgnoreQueryFilters()
                .Where(p => p.Photos.Any(p => p.Id == photoId))
                .FirstOrDefaultAsync();
        }


        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            ArgumentException.ThrowIfNullOrEmpty(username);

            return await this.context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await this.context.Users
                .Include(p => p.Photos)
                .ToListAsync();
        }


        public void Update(AppUser user)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            this.context.Entry(user).State = EntityState.Modified;
        }
    }
}