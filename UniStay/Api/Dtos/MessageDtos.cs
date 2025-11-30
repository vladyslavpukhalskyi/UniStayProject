// Рекомендується розмістити кожен record в окремому файлі
// Наприклад: Api/Dtos/MessageDto.cs, Api/Dtos/CreateMessageDto.cs, Api/Dtos/UpdateMessageDto.cs

// Потрібні using для доменних сутностей та їх ID
using Domain.Messages;
using Domain.Users; // Для UserId та UserSummaryDto
using System; // Для Guid та DateTime

// Переконайтесь, що UserSummaryDto доступний у цьому контексті.
// Якщо він визначений в іншому файлі DTO (напр., ReviewDtos),
// цей using вже повинен бути неявно доступним через namespace Api.Dtos.
// Якщо ні, визначте його тут або в окремому файлі UserSummaryDto.cs:
// public record UserSummaryDto(Guid Id, string FullName) { /* ... FromDomainModel ... */ }

namespace Api.Dtos
{
    /// <summary>
    /// DTO для відображення інформації про повідомлення.
    /// Включає коротку інформацію про відправника та отримувача.
    /// </summary>
    public record MessageDto(
        Guid Id,
        Guid SenderId,
        UserSummaryDto? Sender, // Коротка інформація про відправника
        Guid ReceiverId,
        UserSummaryDto? Receiver, // Коротка інформація про отримувача
        string Text,
        DateTime SendDate)
    {
        /// <summary>
        /// Статичний метод для створення MessageDto з доменної моделі Message.
        /// Передбачає, що навігаційні властивості 'Sender' та 'Receiver' можуть бути завантажені.
        /// </summary>
        public static MessageDto FromDomainModel(Message message) =>
            new(
                Id: message.Id.Value,
                SenderId: message.SenderId.Value,
                Sender: message.Sender == null ? null : UserSummaryDto.FromDomainModel(message.Sender),
                ReceiverId: message.ReceiverId.Value,
                Receiver: message.Receiver == null ? null : UserSummaryDto.FromDomainModel(message.Receiver),
                Text: message.Text,
                SendDate: message.SendDate
            );
    }

    /// <summary>
    /// DTO для створення (відправлення) нового повідомлення.
    /// SenderId зазвичай визначається на основі аутентифікованого користувача.
    /// </summary>
    public record CreateMessageDto(
        Guid ReceiverId, // Кому надсилається повідомлення
        string Text      // Текст повідомлення
    );

    /// <summary>
    /// DTO для оновлення тексту існуючого повідомлення.
    /// Оновлення повідомлень може бути обмеженим або відсутнім у деяких системах.
    /// </summary>
    public record UpdateMessageDto(
        string? Text // Новий текст повідомлення (null = не змінювати)
        // Id повідомлення передається в URL.
    );

    // Порожній клас MessageDtos тепер не потрібен.
    // public class MessageDtos { }
}