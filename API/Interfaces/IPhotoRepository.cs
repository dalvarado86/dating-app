using API.Entities;
using API.Entities.DTOs;

namespace API.Interfaces
{
    public interface IPhotoRepository
    {
        Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotosAsync();
        Task<Photo> GetPhotoByIdAsync(int id);
        void RemovePhoto(Photo photo);
    }
}