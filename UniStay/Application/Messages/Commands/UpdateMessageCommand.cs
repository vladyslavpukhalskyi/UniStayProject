// Файл: Application/Messages/Commands/UpdateMessageCommand.cs
using Application.Common; // Для Result
using Application.Common.Interfaces.Repositories; // Для IMessagesRepository
using Application.Messages.Exceptions; // Для MessageException та підтипів
using Domain.Messages; // Для Message, MessageId
using Domain.Users;   // Для UserId
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Messages.Commands
{
    /// <summary>
    /// Команда для оновлення тексту існуючого повідомлення.
    /// </summary>
    public record UpdateMessageCommand : IRequest<Result<Message, MessageException>>
    {
        /// <summary>
        /// ID повідомлення, яке оновлюється.
        /// </summary>
        public required Guid MessageId { get; init; }

        /// <summary>
        /// Новий текст повідомлення.
        /// </summary>
        public required string Text { get; init; }

        /// <summary>
        /// ID користувача, який запитує оновлення (для перевірки авторизації).
        /// Встановлюється з контексту аутентифікації.
        /// </summary>
        public required Guid RequestingUserId { get; init; }
    }

    /// <summary>
    /// Обробник команди UpdateMessageCommand.
    /// </summary>
    public class UpdateMessageCommandHandler(
        IMessagesRepository messagesRepository)
        : IRequestHandler<UpdateMessageCommand, Result<Message, MessageException>>
    {
        public async Task<Result<Message, MessageException>> Handle(UpdateMessageCommand request, CancellationToken cancellationToken)
        {
            var messageIdToUpdate = new MessageId(request.MessageId);
            var requestingUserId = new UserId(request.RequestingUserId);

            // 1. Отримати повідомлення за ID
            var existingMessageOption = await messagesRepository.GetById(messageIdToUpdate, cancellationToken);

            return await existingMessageOption.Match<Task<Result<Message, MessageException>>>(
                some: async message => // Якщо повідомлення знайдено
                {
                    // 2. Перевірка авторизації: чи є поточний користувач відправником повідомлення
                    if (message.SenderId != requestingUserId)
                    {
                        return new UserNotAuthorizedForMessageOperationException(requestingUserId, messageIdToUpdate, "UpdateMessage");
                    }
                    
                    // (Додатково можна додати перевірку, чи минув дозволений час для редагування, якщо таке правило існує)

                    // 3. Оновити сутність повідомлення
                    return await UpdateMessageEntity(message, request, cancellationToken);
                },
                none: () => // Якщо повідомлення не знайдено
                {
                    MessageException exception = new MessageNotFoundException(messageIdToUpdate);
                    return Task.FromResult<Result<Message, MessageException>>(exception);
                }
            );
        }

        private async Task<Result<Message, MessageException>> UpdateMessageEntity(Message message, UpdateMessageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 4. Оновити текст повідомлення за допомогою методу доменної моделі
                message.UpdateText(request.Text);

                // 5. Зберегти оновлене повідомлення через репозиторій
                var updatedMessage = await messagesRepository.Update(message, cancellationToken);
                return updatedMessage; // Implicit conversion
            }
            catch (Exception exception)
            {
                // 6. Обробити можливі помилки під час збереження
                return new MessageOperationFailedException(message.Id, "UpdateMessage", exception);
            }
        }
    }
}