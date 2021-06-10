using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using API.DTOs;
using API.Helpers;
using API.Extensions;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using API.Controllers;
using Microsoft.AspNetCore.Http;
namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPhotoRepository _photoRepository;

        public UserRepository(DataContext context, IMapper mapper,
        IHttpContextAccessor httpContextAccessor, IPhotoRepository photoRepository)
        {
            _photoRepository = photoRepository;
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<MemberDto> GetMemberAsync(string username)
        {
            var CurrentUsername = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);//user.UserName;//ClaimsPrincipleExtensions.GetUsername(user);
            if (CurrentUsername == username)
            {
                return await _context.Users
                .Where(x => x.UserName == username)
                .IgnoreQueryFilters()
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
            }
            else
            {
                return await _context.Users
               .Where(x => x.UserName == username)
               .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
               .SingleOrDefaultAsync();
            }
        }
        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = _context.Users.AsQueryable();

            query = query.Where(u => u.UserName != userParams.CurrentUsername);
            query = query.Where(u => u.Gender == userParams.Gender);

            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

            query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(u => u.LastActive)
            };
            return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(_mapper
            .ConfigurationProvider).AsNoTracking(),
             userParams.PageNumber, userParams.PageSize);


        }



        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByPhotoId(int id)
        {
            //var photos = await _context.Photos.Where( p=> true==true).IgnoreQueryFilters().ToListAsync();
            var photos = await _photoRepository.GetUnapprovedPhotos();
            var photo =  photos.FirstOrDefault(p=>p.Id==id);//_context.Photos.Take(1).Where(p => p.Id == id).IgnoreQueryFilters() as Photo;
            
            //var user = _context.Users.FirstOrDefault(u => u.Id == photo.AppUserId);
            return await _context.Users.FindAsync(photo.AppUserId);
        }
        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users
            .Include(p => p.Photos)
            .ToListAsync();
        }


        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }

        public async Task<string> GetUserGender(string username)
        {
            return await _context.Users
            .Where(u => u.UserName == username)
            .Select(x => x.Gender).FirstOrDefaultAsync();
        }
    }
}