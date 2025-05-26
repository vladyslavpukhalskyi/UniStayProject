using Application.Common; 
using Application.Common.Interfaces.Repositories; 
using Application.Messages.Exceptions; 
using Domain.Messages; 
using Domain.Users;   
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Messages.Commands
{
    public record DeleteMessageCommand : IRequest<Result<Message, MessageException>>
    {
        public required Guid MessageId { get; init; }

        public required Guid RequestingUserId { get; init; }
    }

    public class DeleteMessageCommandHandler(
        IMessagesRepository messagesRepository)
        : IRequestHandler<DeleteMessageCommand, Result<Message, MessageException>>
    {
        public async Task<Result<Message, MessageException>> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
        {
            var messageIdToDelete = new MessageId(request.MessageId);
            var requestingUserId = new UserId(request.RequestingUserId);

            var existingMessageOption = await messagesRepository.GetById(messageIdToDelete, cancellationToken);

            return await existingMessageOption.Match<Task<Result<Message, MessageException>>>(
                some: async message => 
                {
                    if (message.SenderId != requestingUserId)
                    {
                        return new UserNotAuthorizedForMessageOperationException(requestingUserId, messageIdToDelete, "DeleteMessage");
                    }

                    return await DeleteMessageEntity(message, cancellationToken);
                },
                none: () => 
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
                var deletedMessage = await messagesRepository.Delete(message, cancellationToken);
                return deletedMessage; 
            }
            catch (Exception exception)
            {
                return new MessageOperationFailedException(message.Id, "DeleteMessage", exception);
            }
        }
    }
}