using Domain.Chats;
using Domain.Users;
using Optional;

namespace Application.Common.Interfaces.Queries
{
    public interface IChatsQueries
    {
        Task<Option<Chat>> GetById(ChatId id, CancellationToken cancellationToken);
        Task<IReadOnlyList<Chat>> GetUserChats(UserId userId, CancellationToken cancellationToken);
        Task<IReadOnlyList<Chat>> GetAllChats(CancellationToken cancellationToken);
        Task<IReadOnlyList<ChatMessage>> GetChatMessages(ChatId chatId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ChatMember>> GetChatMembers(ChatId chatId, CancellationToken cancellationToken);
        Task<bool> IsUserMember(ChatId chatId, UserId userId, CancellationToken cancellationToken);
    }
}
