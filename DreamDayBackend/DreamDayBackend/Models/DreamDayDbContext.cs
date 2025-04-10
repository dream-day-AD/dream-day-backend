using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DreamDayBackend.Models
{
    public class DreamDayDbContext : IdentityDbContext<User>
    {
        public DreamDayDbContext(DbContextOptions<DreamDayDbContext> options)
            : base(options)
        {
        }

        public DbSet<Guest> Guests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<Guest>()
                .HasOne(g => g.User)
                .WithMany()
                .HasForeignKey(g => g.UserId);
        }
    }
}