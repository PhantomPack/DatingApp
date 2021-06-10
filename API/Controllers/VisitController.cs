using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using API.Extensions;

namespace API.Controllers
{
    public class VisitController : BaseApiController
    {
        private readonly IVisitsRepository _visitsRepository;
        private readonly IUserRepository _userRepository;
        public VisitController(IUserRepository userRepository, IVisitsRepository visitsRepository)
        {
            _userRepository = userRepository;
            _visitsRepository = visitsRepository;

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VisitDto>>> GetUserVisits(string predicate){
            var users = await _visitsRepository.GetUserVisits(predicate, User.GetUserId()); 

            return Ok(users);        
        }
    }
}