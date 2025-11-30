using Domain.Chats;
using Domain.Users;
using Optional;

namespace Application.Common.Interfaces.Repositories
{
    public interface IChatMessagesRepository
    {
        Task<ChatMessage> Add(ChatMessage message, CancellationToken cancellationToken);
        Task<ChatMessage> Update(ChatMessage message, CancellationToken cancellationToken);
        Task<ChatMessage> Delete(ChatMessage message, CancellationToken cancellationToken);
        Task<Option<ChatMessage>> GetById(ChatMessageId id, CancellationToken cancellationToken);
        Task<IReadOnlyList<ChatMessage>> GetChatMessages(ChatId chatId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ChatMessage>> GetUserMessages(UserId userId, CancellationToken cancellationToken);
        Task<int> GetChatMessageCount(ChatId chatId, CancellationToken cancellationToken);
    }
}
