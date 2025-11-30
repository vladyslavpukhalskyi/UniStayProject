using Application.Common; 
using Application.Common.Interfaces.Repositories; 
using Application.Messages.Exceptions; 
using Domain.Messages; 
using Domain.Users;   
using MediatR;

namespace Application.Messages.Commands
{
    public record CreateMessageCommand : IRequest<Result<Message, MessageException>>
    {
        public required Guid ReceiverId { get; init; }

        public required string Text { get; init; }

        public required Guid SenderId { get; init; }
    }

    public class CreateMessageCommandHandler(
        IMessagesRepository messagesRepository,
        IUsersRepository usersRepository 
        )
        : IRequestHandler<CreateMessageCommand, Result<Message, MessageException>>
    {
        public async Task<Result<Message, MessageException>> Handle(CreateMessageCommand request, CancellationToken cancellationToken)
        {
            var receiverId = new UserId(request.ReceiverId);
            var senderId = new UserId(request.SenderId);

            if (senderId == receiverId)
            {
                return new CannotSendMessageToSelfException(senderId);
            }

            var receiverOption = await usersRepository.GetById(receiverId, cancellationToken);
            if (!receiverOption.HasValue)
            {
                return new ReceiverNotFoundException(receiverId);
            }

            var messageId = MessageId.New();
            try
            {
                var message = Message.New(
                    id: messageId,
                    senderId: senderId,
                    receiverId: receiverId,
                    text: request.Text
                );

                var addedMessage = await messagesRepository.Add(message, cancellationToken);
                return addedMessage; 
            }
            catch (Exception exception)
            {
                return new MessageOperationFailedException(messageId, "CreateMessage", exception);
            }
        }
    }
}