using Domain.Listings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class OstrohLandmarksConfiguration : IEntityTypeConfiguration<OstrohLandmark>
    {
        public void Configure(EntityTypeBuilder<OstrohLandmark> builder)
        {
            builder.ToTable("OstrohLandmarks");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Id)
                .HasConversion(
                    id => id.Value,
                    value => new OstrohLandmarkId(value))
                .ValueGeneratedNever();

            builder.Property(l => l.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(l => l.Address)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(l => l.Latitude)
                .IsRequired();

            builder.Property(l => l.Longitude)
                .IsRequired();
        }
    }
}
