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



    }
}
