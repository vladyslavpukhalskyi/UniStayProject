using Api.Dtos;

namespace Api.Hubs
{
    /// <summary>
    /// Інтерфейс для методів, які сервер може викликати на клієнті
    /// </summary>
    public interface IChatClient
    {
        /// <summary>
        /// Отримання нового повідомлення в чаті
        /// </summary>
        Task ReceiveMessage(ChatMessageDto message);

        /// <summary>
        /// Повідомлення було відредаговано
        /// </summary>
        Task MessageEdited(ChatMessageDto message);

        /// <summary>
        /// Повідомлення було видалено
        /// </summary>
        Task MessageDeleted(Guid chatId, Guid messageId);

        /// <summary>
        /// Користувач приєднався до чату
        /// </summary>
        Task UserJoined(Guid chatId, ChatMemberDto member);

        /// <summary>
        /// Користувач покинув чат
        /// </summary>
        Task UserLeft(Guid chatId, Guid userId);

        /// <summary>
        /// Користувач почав друкувати
        /// </summary>
        Task UserTyping(Guid chatId, Guid userId, string userName);

        /// <summary>
        /// Користувач перестав друкувати
        /// </summary>
        Task UserStoppedTyping(Guid chatId, Guid userId);
    }
}
