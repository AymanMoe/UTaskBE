using Microsoft.EntityFrameworkCore;
using UTask.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace UTask.Data.Contexts
{
    public class UTaskDbContext : IdentityDbContext
    {
        public UTaskDbContext(DbContextOptions<UTaskDbContext> options) : base(options)
        {
        }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProviderCategory> ProviderCategories { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Address> Addresses { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AppUser>().HasOne(l => l.ProviderDetails)
                .WithOne(p => p.AppUser)
                .HasForeignKey<Provider>(c => c.AppUserName).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<AppUser>().HasOne(l => l.ClientDetails)
                .WithOne(p => p.AppUser)
                .HasForeignKey<Client>(c => c.AppUserName).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Booking>().HasOne(l => l.Provider)
                .WithMany(p => p.Bookings)
                .HasForeignKey(c => c.ProviderId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Booking>().HasOne(l => l.Client)
                .WithMany(p => p.Bookings)
                .HasForeignKey(c => c.ClientId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Category>().HasOne(l => l.Booking)
                .WithOne(p => p.Category)
                .HasForeignKey<Booking>(c => c.CategoryId);
            modelBuilder.Entity<ProviderCategory>().ToTable("Subscription");
            modelBuilder.Entity<ProviderCategory>().HasKey(pc => new { pc.ProviderId, pc.CategoryId });

            modelBuilder.Entity<AppUser>().HasOne(l => l.Address)
                .WithOne(p => p.AppUser)
                .HasForeignKey<Address>(c => c.AppUserName).OnDelete(DeleteBehavior.Cascade);
        }
    
    }
}
