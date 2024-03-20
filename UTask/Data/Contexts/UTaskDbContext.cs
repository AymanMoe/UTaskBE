using Microsoft.EntityFrameworkCore;
using UTask.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Composition.Convention;
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
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ClientNotification> ClientNotifications { get; set; }
        public DbSet<ProviderNotification> ProviderNotifications { get; set; }
         public DbSet<ConnectionMapping> ConnectionMappings { get; set; }
        public DbSet<NotifiedProvider> NotifiedProviders { get; set; }
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
            modelBuilder.Entity<Category>().HasMany(l => l.Bookings)
                .WithOne(p => p.Category)
                .HasForeignKey(c => c.CategoryId).OnDelete(DeleteBehavior.ClientNoAction);
            modelBuilder.Entity<Booking>().HasOne(l => l.Client)
                .WithMany(p => p.Bookings)
                .HasForeignKey(c => c.ClientId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Address>().HasMany(l => l.Bookings)
                .WithOne(p => p.Address)
                .HasForeignKey(c => c.AddressId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<ProviderCategory>().ToTable("Subscription");
            modelBuilder.Entity<ProviderCategory>().HasKey(pc => new { pc.ProviderId, pc.CategoryId });

            modelBuilder.Entity<AppUser>().HasOne(l => l.Address)
                .WithOne(p => p.AppUser)
                .HasForeignKey<Address>(c => c.AppUserName).OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<ClientNotification>()
            .HasKey(cn => new { cn.ClientId, cn.NotificationId });

            modelBuilder.Entity<ClientNotification>()
                .HasOne(cn => cn.Client)
                .WithMany(c => c.ClientNotifications)
                .HasForeignKey(cn => cn.ClientId);

            modelBuilder.Entity<ClientNotification>()
                .HasOne(cn => cn.Notification)
                .WithMany(n => n.ClientNotifications)
                .HasForeignKey(cn => cn.NotificationId);

            modelBuilder.Entity<ProviderNotification>()
            .HasKey(pn => new { pn.ProviderId, pn.NotificationId });

            modelBuilder.Entity<ProviderNotification>()
                .HasOne(pn => pn.Provider)
                .WithMany(p => p.ProviderNotifications)
                .HasForeignKey(pn => pn.ProviderId);

            modelBuilder.Entity<ProviderNotification>()
                .HasOne(pn => pn.Notification)
                .WithMany(n => n.ProviderNotifications)
                .HasForeignKey(pn => pn.NotificationId);

            modelBuilder.Entity<ConnectionMapping>()
            .HasOne(uc => uc.User)
            .WithOne(c => c.ConnectionMapping)
            .HasForeignKey<ConnectionMapping>(uc => uc.UserId)
            .IsRequired();


            modelBuilder.Entity<NotifiedProvider>()
                .HasOne(np => np.Booking)
                .WithMany(b => b.NotifiedProviders)
                .HasForeignKey(np => np.BookingId);
        }
    
    }
}
