using Domain.ListingImages;
using Domain.Listings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ListingImagesConfiguration : IEntityTypeConfiguration<ListingImage>
    {
        public void Configure(EntityTypeBuilder<ListingImage> builder)
        {
            builder.ToTable("ListingImages");
            
            builder.HasKey(li => li.Id);
            
            builder.Property(li => li.Id)
                .HasConversion(
                    id => id.Value, 
                    value => new ListingImageId(value))
                .ValueGeneratedNever();
            
            builder.Property(li => li.ListingId)
                .HasConversion(
                    id => id.Value, 
                    value => new ListingId(value))
                .IsRequired();
            
            builder.Property(li => li.ImageUrl)
                .IsRequired()
                .HasMaxLength(2048);
            
            builder.HasOne(li => li.Listing)
                .WithMany(l => l.ListingImages)
                .HasForeignKey(li => li.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}