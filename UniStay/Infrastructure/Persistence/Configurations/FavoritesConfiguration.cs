using Domain.Favorites;
using Domain.Listings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class FavoritesConfiguration : IEntityTypeConfiguration<Favorite>
    {
        public void Configure(EntityTypeBuilder<Favorite> builder)
        {
            builder.ToTable("Favorites");

            builder.HasKey(f => f.Id);
            
            builder.Property(f => f.Id)
                .HasConversion(id => id.Value, value => new FavoriteId(value))
                .ValueGeneratedNever();

            builder.Property(f => f.ListingId)
                .HasConversion(id => id.Value, value => new ListingId(value))
                .IsRequired();

            builder.HasOne(f => f.Listing)
                .WithMany(l => l.Favorites)
                .HasForeignKey(f => f.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(f => f.Users)
                .WithMany(u => u.Favorites)
                .UsingEntity(j => j.ToTable("UserFavorites"));
        }
    }
}