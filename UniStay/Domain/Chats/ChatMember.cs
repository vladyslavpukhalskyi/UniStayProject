using Domain.Users;

namespace Domain.Chats;

public class ChatMember
{
    public ChatMemberId Id { get; }
    public ChatId ChatId { get; private set; }
    public Chat? Chat { get; }
    public UserId UserId { get; private set; }
    public User? User { get; }
    public ChatEnums.ChatMemberRole Role { get; private set; }
    public DateTime JoinedAt { get; }
    public DateTime? LeftAt { get; private set; }
    public bool IsActive { get; private set; }

    private ChatMember(ChatMemberId id, ChatId chatId, UserId userId, ChatEnums.ChatMemberRole role, DateTime joinedAt)
    {
        Id = id;
        ChatId = chatId;
        UserId = userId;
        Role = role;
        JoinedAt = joinedAt;
        IsActive = true;
    }

    public static ChatMember New(ChatMemberId id, ChatId chatId, UserId userId, ChatEnums.ChatMemberRole role)
        => new(id, chatId, userId, role, DateTime.UtcNow);

    public void ChangeRole(ChatEnums.ChatMemberRole newRole)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot change role of inactive member");
            
        Role = newRole;
    }

    public void Leave()
    {
        IsActive = false;
        LeftAt = DateTime.UtcNow;
    }

    public void Rejoin()
    {
        IsActive = true;
        LeftAt = null;
    }
}
