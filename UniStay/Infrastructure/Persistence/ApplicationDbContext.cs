using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Domain.Amenities;
using Domain.Chats;
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
        
        public DbSet<User> Users { get; set; }
        public DbSet<Listing> Listings { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ListingImage> ListingImages { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatMember> ChatMembers { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<OstrohLandmark> OstrohLandmarks { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly()); 
            ConfigureRelations(modelBuilder); 

            base.OnModelCreating(modelBuilder);
        }
        
        private void ConfigureRelations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Listing>()
                .HasOne(l => l.User)
                .WithMany(u => u.Listings)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);  
            
            modelBuilder.Entity<Listing>()
                .HasMany(l => l.Amenities)
                .WithMany(a => a.Listings)
                .UsingEntity(j => j.ToTable("ListingAmenities"));
            
            modelBuilder.Entity<Listing>()
                .HasMany(l => l.Reviews)
                .WithOne(r => r.Listing)
                .HasForeignKey(r => r.ListingId);
            
            modelBuilder.Entity<User>()
                .HasMany(u => u.Reviews)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId);
            
            modelBuilder.Entity<Listing>()
                .HasMany(l => l.ListingImages)
                .WithOne(i => i.Listing)
                .HasForeignKey(i => i.ListingId);
            
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Listing)
                .WithMany(l => l.Favorites)
                .HasForeignKey(f => f.ListingId);
            
            modelBuilder.Entity<Favorite>()
                .HasMany(f => f.Users)
                .WithMany(u => u.Favorites)
                .UsingEntity(j => j.ToTable("UserFavorites"));
            
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
