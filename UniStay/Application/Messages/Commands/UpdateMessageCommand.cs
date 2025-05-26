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
    public record UpdateMessageCommand : IRequest<Result<Message, MessageException>>
    {
        public required Guid MessageId { get; init; }

        public required string Text { get; init; }

        public required Guid RequestingUserId { get; init; }
    }

    public class UpdateMessageCommandHandler(
        IMessagesRepository messagesRepository)
        : IRequestHandler<UpdateMessageCommand, Result<Message, MessageException>>
    {
        public async Task<Result<Message, MessageException>> Handle(UpdateMessageCommand request, CancellationToken cancellationToken)
        {
            var messageIdToUpdate = new MessageId(request.MessageId);
            var requestingUserId = new UserId(request.RequestingUserId);

            var existingMessageOption = await messagesRepository.GetById(messageIdToUpdate, cancellationToken);

            return await existingMessageOption.Match<Task<Result<Message, MessageException>>>(
                some: async message => 
                {
                    if (message.SenderId != requestingUserId)
                    {
                        return new UserNotAuthorizedForMessageOperationException(requestingUserId, messageIdToUpdate, "UpdateMessage");
                    }
                    
                    return await UpdateMessageEntity(message, request, cancellationToken);
                },
                none: () => 
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
                message.UpdateText(request.Text);

                var updatedMessage = await messagesRepository.Update(message, cancellationToken);
                return updatedMessage; 
            }
            catch (Exception exception)
            {
                return new MessageOperationFailedException(message.Id, "UpdateMessage", exception);
            }
        }
    }
}