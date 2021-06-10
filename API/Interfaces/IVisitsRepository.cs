using System.Threading.Tasks;
using API.Entities;
using System.Collections.Generic;
using API.DTOs;

namespace API.Interfaces
{
    public interface IVisitsRepository
    {
         Task<UserVisit> GetUserVisit(int sourceUserId, int visitedUserId);
         Task<AppUser> GetUserWithVisits(int userId);
         Task<IEnumerable<VisitDto>> GetUserVisits(string predicate, int userId);
    }
}