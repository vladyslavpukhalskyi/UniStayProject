using Domain.Listings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ListingsConfiguration : IEntityTypeConfiguration<Listing>
    {
        public void Configure(EntityTypeBuilder<Listing> builder)
        {
            builder.ToTable("Listings");

            builder.HasKey(l => l.Id);
            
            builder.Property(l => l.Id)
                .HasConversion(
                    id => id.Value,
                    value => new ListingId(value))
                .ValueGeneratedNever();

            builder.Property(l => l.Title)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(l => l.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(l => l.Address)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(l => l.Price)
                .IsRequired();

            builder.Property(l => l.Type)
                .IsRequired();

            builder.Property(l => l.Owners)
                .IsRequired();
            
            builder.Property(l => l.Neighbours)
                .IsRequired();
            
            builder.Property(l => l.PublicationDate)
                .IsRequired();

            builder.HasOne(l => l.User)
                .WithMany(u => u.Listings)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(l => l.Amenities)
                .WithMany(a => a.Listings)
                .UsingEntity(j => j.ToTable("ListingAmenities"));

            builder.HasMany(l => l.Reviews)
                .WithOne(r => r.Listing)
                .HasForeignKey(r => r.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(l => l.ListingImages)
                .WithOne(i => i.Listing)
                .HasForeignKey(i => i.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}