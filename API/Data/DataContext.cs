using Microsoft.EntityFrameworkCore;
using API.Entities;
namespace API.Data
{
    public class DataContext: DbContext
    {
        public DataContext(DbContextOptions options): base(options)
        {
        }
        public DbSet<AppUser> Users { get; set; }
        public DbSet <UserLike> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    // Specify primary key
    builder.Entity<UserLike>()
        .HasKey(k => new {k.SourceUserId, k.LikedUserId});

    // Set the relationships
    builder.Entity<UserLike>()
        .HasOne(s => s.SourceUser)
        .WithMany(l => l.LikedUsers)
        .HasForeignKey(s => s.SourceUserId)
        .OnDelete(DeleteBehavior.Cascade);
        // Note: For SQL Server, set the DeleteBehavior to
        // DeleteBehavior.NoAction or you will get an error during migration.

    builder.Entity<UserLike>()
        .HasOne(s => s.LikedUser)
        .WithMany(l => l.LikedByUsers)
        .HasForeignKey(s => s.LikedUserId)
        .OnDelete(DeleteBehavior.Cascade);
}



    }
}