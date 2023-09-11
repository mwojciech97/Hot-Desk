using HotDeskAPI.Model;
using HotDeskAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotDeskAPI.DataContext
{
    public class HotDeskDBContext : DbContext
    {
        public HotDeskDBContext(DbContextOptions<HotDeskDBContext> options) : base(options) { }
        public DbSet<Desk> Desks { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .HasOne(u => u.User)
                .WithOne()
                .HasForeignKey<Employee>(e => e.UserId);

            modelBuilder.Entity<Desk>()
                .HasOne(d => d.Location)
                .WithMany()
                .HasForeignKey(d => d.LocationId);
        }
    }
}
