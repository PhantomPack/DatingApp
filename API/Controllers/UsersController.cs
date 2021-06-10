using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using API.Interfaces;
using API.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using API.Extensions;
using API.Helpers;
using Microsoft.AspNetCore.Identity;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly DataContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IVisitsRepository _visitsRepository;
        public UsersController(IVisitsRepository visitsRepository, IUserRepository userRepository, IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService, DataContext context, UserManager<AppUser> userManager)
        {
            _visitsRepository = visitsRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _photoService = photoService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {

            var gender = await _unitOfWork.UserRepository.GetUserGender(User.GetUsername());
            userParams.CurrentUsername = User.GetUsername();

            if (string.IsNullOrEmpty(userParams.Gender))
                userParams.Gender = gender == "male" ? "female" : "male";



            userParams.CurrentUsername = User.GetUsername();



            var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(users.CurrentPage,
             users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }

        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
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
            var user = users.FirstOrDefault(u => u.Username == username);

            var sourceUserId = User.GetUserId();
            var visitedUser = await _userRepository.GetUserByUsernameAsync(username);
            var sourceUser = await _visitsRepository.GetUserWithVisits(sourceUserId);

            if (User.IsInRole("VIP"))
            {
                if (visitedUser != null && sourceUser.UserName != username)
                {
                    var userVisit = await _visitsRepository.GetUserVisit(sourceUserId, visitedUser.Id);
                    if (userVisit == null)
                    {
                        userVisit = new UserVisit
                        {
                            SourceUserId = sourceUserId,
                            VisitedUserId = visitedUser.Id
                        };
                        sourceUser.VisitedUsers.Add(userVisit);

                        if (await _unitOfWork.Complete())
                        {

                        }
                        else return BadRequest("Failed to add a visit");
                    }

                }
            }
            if (user.Roles.IndexOf("VIP") >= 0)
            {
                sourceUserId = User.GetUserId();
                sourceUser = await _userRepository.GetUserByUsernameAsync(username);
                visitedUser = await _visitsRepository.GetUserWithVisits(sourceUserId);
                if (visitedUser != null && visitedUser.UserName != username)
                {
                    var userVisit = await _visitsRepository.GetUserVisit(sourceUserId, visitedUser.Id);
                    if (userVisit == null)
                    {
                        userVisit = new UserVisit
                        {
                            SourceUserId = visitedUser.Id,
                            VisitedUserId = sourceUserId
                        };

                        if (sourceUser.VisitedByUsers == null)
                        {
                            sourceUser.VisitedByUsers = new List<UserVisit>();
                            sourceUser.VisitedByUsers.Add(userVisit);
                        }
                        else
                        {
                            if (_visitsRepository.GetUserVisit(userVisit.SourceUserId, userVisit.VisitedUserId) == null)
                            {
                                sourceUser.VisitedByUsers.Add(userVisit);
                            }
                        }

                        if (await _unitOfWork.Complete())
                        {

                        }
                    }

                }
            }



            return await _unitOfWork.UserRepository.GetMemberAsync(username);


        }

        // api/users
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            _mapper.Map(memberUpdateDto, user);

            _unitOfWork.UserRepository.Update(user);

            if (await _unitOfWork.Complete()) return NoContent();
            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            photo.isApproved = false;

            user.Photos.Add(photo);
            _context.Photos.Add(photo);//make sure photo is added in the database

            if (await _unitOfWork.Complete())
            {
                //return CreatedAtRoute("GetUser", _mapper.Map<PhotoDto>(photo));
                return CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo.IsMain) return BadRequest("This is already your main photo");

            if (photo.isApproved)
            {
                var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
                if (currentMain != null) currentMain.IsMain = false;
                photo.IsMain = true;
            }
            else { return BadRequest("Unapproved photos cannot be set to main"); }

            if (await _unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to set main photo");
        }



        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null) return NotFound();
            if (photo.IsMain) return BadRequest("You cannot delete your main photo");
            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }
            user.Photos.Remove(photo);
            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to delete the photo");
        }



    }
}