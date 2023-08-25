using API.Entities;
using API.Entities.DTOs;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext dbContext;

        public PhotoRepository(DataContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<Photo> GetPhotoByIdAsync(int id)
        {
            return await this.dbContext.Photos
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotosAsync()
        {
            return await this.dbContext.Photos
                .IgnoreQueryFilters()
                .Where(p => p.IsApproved == false)
                .Select(u => new PhotoForApprovalDto
                {
                    Id = u.Id,
                    Username = u.AppUser.UserName,
                    Url = u.Url,
                    IsApproved = u.IsApproved
                })
                .ToListAsync();
        }

        public void RemovePhoto(Photo photo)
        {
            ArgumentNullException.ThrowIfNull(photo, nameof(photo));

            this.dbContext.Photos.Remove(photo);
        }
    }
}