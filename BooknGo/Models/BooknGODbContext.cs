using Microsoft.EntityFrameworkCore;

namespace BooknGo.Models
{
    public class BooknGODbContext : DbContext
    {
        public BooknGODbContext(DbContextOptions<BooknGODbContext> options) : base(options)
        {
        }

        public DbSet<Booking> Bookings { get; set; } // FIX: Changed to plural name

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Booking>().ToTable("Booking"); // Explicit table name mapping
        }
    }
}
