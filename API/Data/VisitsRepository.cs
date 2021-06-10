using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using API.Extensions;
namespace API.Data
{
    public class VisitsRepository : IVisitsRepository
    {
        private readonly DataContext _context;
        public VisitsRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<UserVisit> GetUserVisit(int sourceUserId, int visitedUserId)
        {
            return await _context.Visits.FindAsync(sourceUserId, visitedUserId);
        }

        public async Task<IEnumerable<VisitDto>> GetUserVisits(string predicate, int userId)
        {
            var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
            var visits = _context.Visits.AsQueryable();

            if(predicate == "visited"){
                visits = visits.Where(visit => visit.SourceUserId == userId);
                users = visits.Select(visit => visit.VisitedUser);
            }

            if(predicate == "visitedBy"){
                visits = visits.Where(visit => visit.VisitedUserId == userId);
                users = visits.Select(visit => visit.SourceUser);
            }

            return await users.Select(user => new VisitDto
            {
                Username = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
                City = user.City,
                Id = user.Id

            }).ToListAsync();
            
        }

        public async Task<AppUser> GetUserWithVisits(int userId)
        {
            return await _context.Users
                .Include(x => x.VisitedUsers)
                .Include(x => x.VisitedByUsers)
                .FirstOrDefaultAsync(x=> x.Id == userId);

        }
    }
}