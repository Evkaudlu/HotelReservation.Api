using HotelReservation.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Api.Data
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(
            DbContextOptions<HotelDbContext> options)
            : base(options)
        {
        }

        public DbSet<Hotel> Hotels { get; set; }

        public DbSet<Room> Rooms { get; set; }

        public DbSet<Client> Clients { get; set; }

        public DbSet<Booking> Bookings { get; set; }
    }
}
