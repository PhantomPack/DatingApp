using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using API.Entities;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using API.Data;
using API.Interfaces;
using AutoMapper;
using API.DTOs;
using System.Collections.Generic;
namespace API.Controllers
{
    public class AdminController : BaseApiController
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly IPhotoRepository _photoRepository;
        private readonly IUserRepository _userRepository;
        private readonly DataContext _context;

        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly IUnitOfWork _unitOfWork;

        public AdminController(UserManager<AppUser> userManager, IPhotoRepository photoRepository,
          IUserRepository userRepository, DataContext context, IMapper mapper, IPhotoService photoService, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _photoService = photoService;
            _mapper = mapper;
            _context = context;
            _userManager = userManager;
            _photoRepository = photoRepository;
            _userRepository = userRepository;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await _userManager.Users
                .Include(r => r.UserRoles)
                .ThenInclude(r => r.Role)
                .OrderBy(u => u.UserName)
                .Select(u => new
                {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                })
                .ToListAsync();

            return Ok(users);
        }


        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            var selectedRoles = roles.Split(",").ToArray();
            var user = await _userManager.FindByNameAsync(username);

            if (user == null) return NotFound("Could not find user");

            var userRoles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if (!result.Succeeded) return BadRequest("Failed to add to roles");

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if (!result.Succeeded) return BadRequest("Failed to remove from roles");

            return Ok(await _userManager.GetRolesAsync(user));
        }


        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<ActionResult> GetPhotosForApproval()
        {
            return Ok(await _photoRepository.GetUnapprovedPhotos());
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("approve-photo/{id}")]
        public async Task<ActionResult> approvePhoto(int id)
        {
            var unapprovedPhotos = await _photoRepository.GetUnapprovedPhotos();

            foreach (var p in unapprovedPhotos)
            {
                if (p.Id == id)
                {
                    var user = await _userRepository.GetUserByPhotoId(id);
                    p.isApproved = true;
                    var photos = await _context.Photos
                    .Where(p => p.AppUserId == user.Id)
                    .IgnoreQueryFilters()
                    .ToListAsync();

                    foreach (var x in photos)
                    {
                        if (x.IsMain)
                        {
                            return Ok(await _photoRepository.GetUnapprovedPhotos());
                        }
                    }
                    p.IsMain = true;
                    //_mapper.Map<PhotoDto>(photo)
                    return Ok(_mapper.Map<ICollection<Photo>, ICollection<PhotoDto>>(await _photoRepository.GetUnapprovedPhotos()));
                }
            }
            return Ok("Failed to approve Photo");
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("reject-photo/{id}")]
        public async Task<ActionResult> rejectPhoto(int id)
        {
            var unapprovedPhotos = await _photoRepository.GetUnapprovedPhotos();

            foreach (var p in unapprovedPhotos)
            {
                if (p.Id == id)
                {
                    var user = await _userRepository.GetUserByPhotoId(id);

                    var photos = await _context.Photos
                    .Where(p => p.AppUserId == user.Id)
                    .IgnoreQueryFilters()
                    .ToListAsync();
                    var photo = user.Photos.FirstOrDefault(x => x.Id == id);
                    if (photo == null) return NotFound();
                    if (photo.PublicId != null)
                    {
                        var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                        if (result.Error != null) return BadRequest(result.Error.Message);
                    }
                    user.Photos.Remove(photo);
                    _photoRepository.RemovePhoto(photos.FirstOrDefault(p => p.AppUserId == user.Id));
                    if (await _unitOfWork.Complete()) return Ok(_photoRepository.GetUnapprovedPhotos());
                }
            }
            return BadRequest("Failed to reject Photo");
        }

    }
}