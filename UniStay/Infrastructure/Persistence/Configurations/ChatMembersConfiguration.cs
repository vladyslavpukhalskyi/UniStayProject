using Domain.Chats;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ChatMembersConfiguration : IEntityTypeConfiguration<ChatMember>
    {
        public void Configure(EntityTypeBuilder<ChatMember> builder)
        {
            builder.HasKey(cm => cm.Id);

            builder.Property(cm => cm.Id)
                .HasConversion(
                    id => id.Value,
                    value => new ChatMemberId(value))
                .ValueGeneratedNever();

            builder.Property(cm => cm.Role)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(cm => cm.JoinedAt)
                .IsRequired();

            builder.Property(cm => cm.LeftAt);

            builder.Property(cm => cm.IsActive)
                .IsRequired();

            builder.HasOne(cm => cm.Chat)
                .WithMany(c => c.Members)
                .HasForeignKey(cm => cm.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cm => cm.User)
                .WithMany()
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
