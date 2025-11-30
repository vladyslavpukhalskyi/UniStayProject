using Domain.Chats;

namespace Application.Common.Interfaces.Services
{
    /// <summary>
    /// Сервіс для відправки real-time сповіщень через SignalR
    /// </summary>
    public interface IChatNotificationService
    {
        /// <summary>
        /// Відправити нове повідомлення всім учасникам чату
        /// </summary>
        Task NotifyNewMessage(ChatMessage message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Сповістити про редагування повідомлення
        /// </summary>
        Task NotifyMessageEdited(ChatMessage message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Сповістити про видалення повідомлення
        /// </summary>
        Task NotifyMessageDeleted(ChatId chatId, ChatMessageId messageId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Сповістити про приєднання користувача до чату
        /// </summary>
        Task NotifyUserJoined(ChatMember member, CancellationToken cancellationToken = default);

        /// <summary>
        /// Сповістити про вихід користувача з чату
        /// </summary>
        Task NotifyUserLeft(ChatId chatId, Domain.Users.UserId userId, CancellationToken cancellationToken = default);
    }
}
