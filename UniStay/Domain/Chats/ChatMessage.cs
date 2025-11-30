using Domain.Users;

namespace Domain.Chats;

public class ChatMessage
{
    public ChatMessageId Id { get; }
    public ChatId ChatId { get; private set; }
    public Chat? Chat { get; }
    public UserId SenderId { get; private set; }
    public User? Sender { get; }
    public string Content { get; private set; }
    public DateTime SentAt { get; }
    public DateTime? EditedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    private ChatMessage(ChatMessageId id, ChatId chatId, UserId senderId, string content, DateTime sentAt)
    {
        Id = id;
        ChatId = chatId;
        SenderId = senderId;
        Content = content;
        SentAt = sentAt;
        IsDeleted = false;
    }

    public static ChatMessage New(ChatMessageId id, ChatId chatId, UserId senderId, string content)
        => new(id, chatId, senderId, content, DateTime.UtcNow);

    public void EditContent(string newContent)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot edit deleted message");
            
        Content = newContent;
        EditedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        IsDeleted = true;
    }
}
