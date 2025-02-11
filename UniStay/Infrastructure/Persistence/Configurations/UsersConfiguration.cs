using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UsersConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id)
                .HasConversion(id => id.Value, value => new UserId(value));

            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);
            
            builder.Property(u => u.Password)
                .IsRequired();
            
            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(20);
            
            builder.Property(u => u.ProfileImage)
                .HasMaxLength(500);
            
            builder.Property(u => u.RegistrationDate)
                .IsRequired();

            builder.Property(u => u.Role)
                .IsRequired()
                .HasConversion<int>();

            // Связь один ко многим: User -> Reviews
            builder.HasMany(u => u.Reviews)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь один ко многим: User -> Listings
            builder.HasMany(u => u.Listings)
                .WithOne(l => l.User)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Связь один ко многим: User -> SentMessages
            builder.HasMany(u => u.SentMessages)
                .WithOne(m => m.Sender)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.NoAction);
            
            // Связь один ко многим: User -> ReceivedMessages
            builder.HasMany(u => u.ReceivedMessages)
                .WithOne(m => m.Receiver)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}