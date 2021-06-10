namespace API.Entities
{
    public class UserVisit
    {
        public AppUser SourceUser { get; set; }
        public int SourceUserId { get; set; }
        public AppUser VisitedUser { get; set; }
        public int VisitedUserId { get; set; }
    }
}