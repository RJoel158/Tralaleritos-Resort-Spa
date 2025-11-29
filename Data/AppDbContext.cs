using Microsoft.EntityFrameworkCore;
using ResortTralaleritos.Models;

namespace ResortTralaleritos.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        public DbSet<ReservationRoom> ReservationRooms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReservationRoom>()
                .HasKey(rr => new { rr.ReservationId, rr.RoomId });

            modelBuilder.Entity<ReservationRoom>()
                .HasOne(rr => rr.Reservation)
                .WithMany(r => r.ReservationRooms)
                .HasForeignKey(rr => rr.ReservationId);

            modelBuilder.Entity<ReservationRoom>()
                .HasOne(rr => rr.Room)
                .WithMany(r => r.ReservationRooms)
                .HasForeignKey(rr => rr.RoomId);
        }
    }
}
