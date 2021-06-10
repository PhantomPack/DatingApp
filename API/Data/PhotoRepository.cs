using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using API.DTOs;
namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public PhotoRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<ICollection<Photo>> GetUnapprovedPhotos()
        {
            var photos = await _context.Photos
            .Where(p => p.isApproved == false)
            .IgnoreQueryFilters()
            .ToListAsync();

            return photos;
        }

        public async Task<Photo> GetPhotoById(int Id)
        {
            return await _context.Photos.FirstOrDefaultAsync(p => p.Id == Id);
        }

        public void RemovePhoto(Photo photo)
        {
                _context.Photos.Remove(photo);
        }



    }
}