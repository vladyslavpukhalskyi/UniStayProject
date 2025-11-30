using Domain.Chats;
using Domain.Users;
using Optional;

namespace Application.Common.Interfaces.Repositories
{
    public interface IChatMembersRepository
    {
        Task<ChatMember> Add(ChatMember member, CancellationToken cancellationToken);
        Task<ChatMember> Update(ChatMember member, CancellationToken cancellationToken);
        Task<ChatMember> Delete(ChatMember member, CancellationToken cancellationToken);
        Task<Option<ChatMember>> GetById(ChatMemberId id, CancellationToken cancellationToken);
        Task<Option<ChatMember>> GetByChatAndUser(ChatId chatId, UserId userId, CancellationToken cancellationToken);
        Task<IReadOnlyList<ChatMember>> GetChatMembers(ChatId chatId, CancellationToken cancellationToken);
        Task<IReadOnlyList<ChatMember>> GetUserMemberships(UserId userId, CancellationToken cancellationToken);
    }
}
