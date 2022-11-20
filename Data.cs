using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RoomReservation.Model;

namespace RoomReservation
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {}
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<Event> Events => Set<Event>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Room>(room => {
                room.HasKey(r => r.Name);
                room.Property(r => r.Keywords)
                    .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
                room.HasMany(r => r.Reservations)
                    .WithMany(e => e.Reservations)
                    .UsingEntity(j => j.ToTable("Reservation"));
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=room-reservation.db");
        }
    }

}
