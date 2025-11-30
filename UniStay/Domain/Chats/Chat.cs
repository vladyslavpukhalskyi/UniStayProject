using Domain.Users;

namespace Domain.Chats;

public class Chat
{
    public ChatId Id { get; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public ChatEnums.ChatType Type { get; private set; }
    public UserId CreatedById { get; private set; }
    public User? CreatedBy { get; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }
    
    public List<ChatMember> Members { get; private set; } = new();
    public List<ChatMessage> Messages { get; private set; } = new();

    private Chat(ChatId id, string name, string? description, ChatEnums.ChatType type, UserId createdById, DateTime createdAt)
    {
        Id = id;
        Name = name;
        Description = description;
        Type = type;
        CreatedById = createdById;
        CreatedAt = createdAt;
        IsActive = true;
    }

    public static Chat New(ChatId id, string name, string? description, ChatEnums.ChatType type, UserId createdById)
        => new(id, name, description, type, createdById, DateTime.UtcNow);

    public void UpdateDetails(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddMember(ChatMember member)
    {
        if (Members.Any(m => m.UserId == member.UserId && m.IsActive))
            throw new InvalidOperationException("User is already a member of this chat");
            
        Members.Add(member);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveMember(UserId userId)
    {
        var member = Members.FirstOrDefault(m => m.UserId == userId && m.IsActive);
        if (member == null)
            throw new InvalidOperationException("User is not a member of this chat");
            
        member.Leave();
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddMessage(ChatMessage message)
    {
        if (!Members.Any(m => m.UserId == message.SenderId && m.IsActive))
            throw new InvalidOperationException("User is not a member of this chat");
            
        Messages.Add(message);
        UpdatedAt = DateTime.UtcNow;
    }

    public void PromoteMember(UserId userId, ChatEnums.ChatMemberRole newRole)
    {
        var member = Members.FirstOrDefault(m => m.UserId == userId && m.IsActive);
        if (member == null)
            throw new InvalidOperationException("User is not a member of this chat");
            
        member.ChangeRole(newRole);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
