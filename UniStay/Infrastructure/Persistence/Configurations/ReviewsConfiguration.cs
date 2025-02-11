using Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ReviewsConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .HasConversion(
                    id => id.Value,
                    value => new ReviewId(value))
                .ValueGeneratedNever();

            builder.Property(r => r.Rating)
                .IsRequired();

            builder.Property(r => r.Comment)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(r => r.PublicationDate)
                .IsRequired();

            builder.HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Listing)
                .WithMany(l => l.Reviews)
                .HasForeignKey(r => r.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}