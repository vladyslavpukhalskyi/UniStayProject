// Файл: Application/Messages/Commands/CreateMessageCommand.cs
using Application.Common; // Для Result
using Application.Common.Interfaces.Repositories; // Для IMessagesRepository, IUsersRepository
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
    /// Команда для створення (відправлення) нового повідомлення.
    /// </summary>
    public record CreateMessageCommand : IRequest<Result<Message, MessageException>>
    {
        /// <summary>
        /// ID отримувача повідомлення.
        /// </summary>
        public required Guid ReceiverId { get; init; }

        /// <summary>
        /// Текст повідомлення.
        /// </summary>
        public required string Text { get; init; }

        /// <summary>
        /// ID відправника повідомлення. Встановлюється з контексту аутентифікації.
        /// </summary>
        public required Guid SenderId { get; init; }
    }

    /// <summary>
    /// Обробник команди CreateMessageCommand.
    /// </summary>
    public class CreateMessageCommandHandler(
        IMessagesRepository messagesRepository,
        IUsersRepository usersRepository // Потрібен для перевірки існування отримувача
        )
        : IRequestHandler<CreateMessageCommand, Result<Message, MessageException>>
    {
        public async Task<Result<Message, MessageException>> Handle(CreateMessageCommand request, CancellationToken cancellationToken)
        {
            var receiverId = new UserId(request.ReceiverId);
            var senderId = new UserId(request.SenderId);

            // 1. Перевірити, чи відправник не є отримувачем
            if (senderId == receiverId)
            {
                return new CannotSendMessageToSelfException(senderId);
            }

            // 2. Перевірити, чи існує отримувач (User)
            var receiverOption = await usersRepository.GetById(receiverId, cancellationToken);
            if (!receiverOption.HasValue)
            {
                return new ReceiverNotFoundException(receiverId);
            }

            var messageId = MessageId.New();
            try
            {
                // 3. Створити сутність Message
                var message = Message.New(
                    id: messageId,
                    senderId: senderId,
                    receiverId: receiverId,
                    text: request.Text
                );

                // 4. Додати повідомлення в репозиторій
                var addedMessage = await messagesRepository.Add(message, cancellationToken);
                return addedMessage; // Implicit conversion
            }
            catch (Exception exception)
            {
                // 5. Обробити можливі помилки під час збереження
                return new MessageOperationFailedException(messageId, "CreateMessage", exception);
            }
        }
    }
}