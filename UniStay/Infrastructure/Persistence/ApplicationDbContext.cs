using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Domain.Amenities;
using Domain.Favorites;
using Domain.ListingImages;
using Domain.Listings;
using Domain.Messages;
using Domain.Reviews;
using Domain.Users;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSet для кожної сутності
        public DbSet<User> Users { get; set; }
        public DbSet<Listing> Listings { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ListingImage> ListingImages { get; set; }

        // Метод налаштування моделі
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly()); // застосовує конфігурації з поточного збірника
            ConfigureRelations(modelBuilder); // додає додаткові відносини між таблицями

            base.OnModelCreating(modelBuilder);
        }

        // Налаштування відносин між таблицями
        private void ConfigureRelations(ModelBuilder modelBuilder)
        {
            // Відношення між Listings і Users (один користувач може мати багато оголошень)
            modelBuilder.Entity<Listing>()
                .HasOne(l => l.User)
                .WithMany(u => u.Listings)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);  // При видаленні користувача всі оголошення також будуть видалені

            // Відношення між Listings і Amenities (багато оголошень можуть мати багато амінностей)
            modelBuilder.Entity<Listing>()
                .HasMany(l => l.Amenities)
                .WithMany(a => a.Listings)
                .UsingEntity(j => j.ToTable("ListingAmenities"));

            // Відношення між Listings і Reviews (одне оголошення може мати багато відгуків)
            modelBuilder.Entity<Listing>()
                .HasMany(l => l.Reviews)
                .WithOne(r => r.Listing)
                .HasForeignKey(r => r.ListingId);

            // Відношення між Users і Reviews (користувач може мати багато відгуків)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Reviews)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId);

            // Відношення між Listings і ListingImages (одне оголошення може мати багато зображень)
            modelBuilder.Entity<Listing>()
                .HasMany(l => l.ListingImages)
                .WithOne(i => i.Listing)
                .HasForeignKey(i => i.ListingId);

            // Відношення між Listings і Favorites (користувачі можуть додавати оголошення в улюблені)
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Listing)
                .WithMany(l => l.Favorites)
                .HasForeignKey(f => f.ListingId);

            // Відношення між Users і Favorites (користувач може мати багато улюблених оголошень)
            modelBuilder.Entity<Favorite>()
                .HasMany(f => f.Users)
                .WithMany(u => u.Favorites)
                .UsingEntity(j => j.ToTable("UserFavorites"));

            // Відношення між Messages і Users (користувач може мати багато повідомлень)
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
