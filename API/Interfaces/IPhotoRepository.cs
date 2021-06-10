using System.Collections.Generic;
using System.Threading.Tasks;
using API.Entities;
using API.DTOs;
namespace API.Interfaces
{
    public interface IPhotoRepository
    {
        Task<ICollection<Photo>> GetUnapprovedPhotos();
         Task<Photo> GetPhotoById(int Id);
          void RemovePhoto(Photo photo);
    }
}
