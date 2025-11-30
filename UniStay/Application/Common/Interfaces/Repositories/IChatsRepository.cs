using Domain.Chats;
using Domain.Users;
using Optional;

namespace Application.Common.Interfaces.Repositories
{
    public interface IChatsRepository
    {
        Task<Chat> Add(Chat chat, CancellationToken cancellationToken);
        Task<Chat> Update(Chat chat, CancellationToken cancellationToken);
        Task<Chat> Delete(Chat chat, CancellationToken cancellationToken);
        Task<Option<Chat>> GetById(ChatId id, CancellationToken cancellationToken);
        Task<IReadOnlyList<Chat>> GetUserChats(UserId userId, CancellationToken cancellationToken);
        Task<IReadOnlyList<Chat>> GetAllChats(CancellationToken cancellationToken);
        Task<bool> IsUserMember(ChatId chatId, UserId userId, CancellationToken cancellationToken);
    }
}
