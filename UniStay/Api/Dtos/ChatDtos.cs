using Domain.Chats;

namespace Api.Dtos
{
    public record ChatDto(
        Guid Id,
        string Name,
        string? Description,
        ChatEnums.ChatType Type,
        Guid CreatedById,
        UserSummaryDto? CreatedBy,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        bool IsActive,
        int MemberCount,
        IReadOnlyList<ChatMemberDto> Members,
        IReadOnlyList<ChatMemberDto> Admins,
        IReadOnlyList<ChatMemberDto> Owners)
    {
        public static ChatDto FromDomainModel(Chat chat) =>
            new(
                Id: chat.Id.Value,
                Name: chat.Name,
                Description: chat.Description,
                Type: chat.Type,
                CreatedById: chat.CreatedById.Value,
                CreatedBy: chat.CreatedBy == null ? null : UserSummaryDto.FromDomainModel(chat.CreatedBy),
                CreatedAt: chat.CreatedAt,
                UpdatedAt: chat.UpdatedAt,
                IsActive: chat.IsActive,
                MemberCount: chat.Members.Count(m => m.IsActive),
                Members: chat.Members
                    .Where(m => m.IsActive)
                    .Select(ChatMemberDto.FromDomainModel)
                    .ToList(),
                Admins: chat.Members
                    .Where(m => m.IsActive && m.Role == ChatEnums.ChatMemberRole.Admin)
                    .Select(ChatMemberDto.FromDomainModel)
                    .ToList(),
                Owners: chat.Members
                    .Where(m => m.IsActive && m.Role == ChatEnums.ChatMemberRole.Owner)
                    .Select(ChatMemberDto.FromDomainModel)
                    .ToList()
            );
    }
    
    public record CreateChatRequest(
        string Name,
        string? Description,
        ChatEnums.ChatType Type
    );
    
    public record UpdateChatRequest(
        string? Name,
        string? Description
    );
    
    public record ChatMemberDto(
        Guid Id,
        Guid ChatId,
        Guid UserId,
        UserSummaryDto? User,
        ChatEnums.ChatMemberRole Role,
        DateTime JoinedAt,
        DateTime? LeftAt,
        bool IsActive)
    {
        public static ChatMemberDto FromDomainModel(ChatMember member) =>
            new(
                Id: member.Id.Value,
                ChatId: member.ChatId.Value,
                UserId: member.UserId.Value,
                User: member.User == null ? null : UserSummaryDto.FromDomainModel(member.User),
                Role: member.Role,
                JoinedAt: member.JoinedAt,
                LeftAt: member.LeftAt,
                IsActive: member.IsActive
            );
    }
    
    public record ChatMessageDto(
        Guid Id,
        Guid ChatId,
        Guid SenderId,
        UserSummaryDto? Sender,
        string Content,
        DateTime SentAt,
        DateTime? EditedAt,
        bool IsDeleted)
    {
        public static ChatMessageDto FromDomainModel(ChatMessage message) =>
            new(
                Id: message.Id.Value,
                ChatId: message.ChatId.Value,
                SenderId: message.SenderId.Value,
                Sender: message.Sender == null ? null : UserSummaryDto.FromDomainModel(message.Sender),
                Content: message.Content,
                SentAt: message.SentAt,
                EditedAt: message.EditedAt,
                IsDeleted: message.IsDeleted
            );
    }
    
    public record SendMessageRequest(
        string Content
    );
    
    public record AddMemberRequest(
        Guid TargetUserId,
        ChatEnums.ChatMemberRole? Role
    );
}
