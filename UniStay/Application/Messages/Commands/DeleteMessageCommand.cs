// Файл: Application/Messages/Commands/DeleteMessageCommand.cs
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
    /// Команда для видалення повідомлення.
    /// </summary>
    public record DeleteMessageCommand : IRequest<Result<Message, MessageException>>
    {
        /// <summary>
        /// ID повідомлення, яке потрібно видалити.
        /// </summary>
        public required Guid MessageId { get; init; }

        /// <summary>
        /// ID користувача, який запитує видалення (для перевірки авторизації).
        /// Встановлюється з контексту аутентифікації.
        /// </summary>
        public required Guid RequestingUserId { get; init; }
    }

    /// <summary>
    /// Обробник команди DeleteMessageCommand.
    /// </summary>
    public class DeleteMessageCommandHandler(
        IMessagesRepository messagesRepository)
        : IRequestHandler<DeleteMessageCommand, Result<Message, MessageException>>
    {
        public async Task<Result<Message, MessageException>> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
        {
            var messageIdToDelete = new MessageId(request.MessageId);
            var requestingUserId = new UserId(request.RequestingUserId);

            // 1. Отримати повідомлення за ID
            var existingMessageOption = await messagesRepository.GetById(messageIdToDelete, cancellationToken);

            return await existingMessageOption.Match<Task<Result<Message, MessageException>>>(
                some: async message => // Якщо повідомлення знайдено
                {
                    // 2. Перевірка авторизації: чи є поточний користувач відправником повідомлення
                    // (Можна розширити логіку, щоб дозволити видалення отримувачу або адміну)
                    if (message.SenderId != requestingUserId)
                    {
                        return new UserNotAuthorizedForMessageOperationException(requestingUserId, messageIdToDelete, "DeleteMessage");
                    }

                    // 3. Видалити сутність повідомлення
                    return await DeleteMessageEntity(message, cancellationToken);
                },
                none: () => // Якщо повідомлення не знайдено
                {
                    MessageException exception = new MessageNotFoundException(messageIdToDelete);
                    return Task.FromResult<Result<Message, MessageException>>(exception);
                }
            );
        }

        private async Task<Result<Message, MessageException>> DeleteMessageEntity(Message message, CancellationToken cancellationToken)
        {
            try
            {
                // 4. Видалити повідомлення через репозиторій
                var deletedMessage = await messagesRepository.Delete(message, cancellationToken);
                // Повертаємо видалене повідомлення (хоча можна повертати Unit, якщо інформація не потрібна)
                return deletedMessage; // Implicit conversion
            }
            catch (Exception exception)
            {
                // 5. Обробити можливі помилки під час видалення
                return new MessageOperationFailedException(message.Id, "DeleteMessage", exception);
            }
        }
    }
}