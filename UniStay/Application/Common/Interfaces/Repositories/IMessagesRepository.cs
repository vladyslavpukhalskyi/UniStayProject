using Domain.Messages;
using Optional;
using Domain.Users;

namespace Application.Common.Interfaces.Repositories
{
    public interface IMessagesRepository
    {
        Task<Message> Add(Message message, CancellationToken cancellationToken);
        Task<Message> Update(Message message, CancellationToken cancellationToken);
        Task<Message> Delete(Message message, CancellationToken cancellationToken);
        Task<Option<Message>> GetById(MessageId id, CancellationToken cancellationToken);
        Task<IReadOnlyList<Message>> GetAllMessagesForUser(UserId userId, CancellationToken cancellationToken);
        Task<IReadOnlyList<Message>> GetConversation(UserId senderId, UserId receiverId, CancellationToken cancellationToken);
    }
}