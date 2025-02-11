using Domain.Amenities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class AmenitiesConfiguration : IEntityTypeConfiguration<Amenity>
    {
        public void Configure(EntityTypeBuilder<Amenity> builder)
        {
            builder.HasKey(a => a.Id);
            
            builder.Property(a => a.Id)
                .HasConversion(id => id.Value, value => new AmenityId(value))
                .IsRequired();
            
            builder.Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(200);
            
            builder.HasMany(a => a.Listings)
                .WithMany(l => l.Amenities)
                .UsingEntity(j => j.ToTable("ListingAmenities"));
        }
    }
}