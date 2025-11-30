using Domain.Chats;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ChatMessagesConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.HasKey(cm => cm.Id);

            builder.Property(cm => cm.Id)
                .HasConversion(
                    id => id.Value,
                    value => new ChatMessageId(value))
                .ValueGeneratedNever();

            builder.Property(cm => cm.Content)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(cm => cm.SentAt)
                .IsRequired();

            builder.Property(cm => cm.EditedAt);

            builder.Property(cm => cm.IsDeleted)
                .IsRequired();

            builder.HasOne(cm => cm.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(cm => cm.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cm => cm.Sender)
                .WithMany()
                .HasForeignKey(cm => cm.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(cm => cm.ChatId);
            builder.HasIndex(cm => cm.SentAt);
        }
    }
}
