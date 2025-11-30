using Application.Common;
using Application.Chats.Exceptions;
using Application.Common.Interfaces.Repositories;
using Domain.Chats;
using Domain.Users;
using MediatR;

namespace Application.Chats.Commands
{
    public record UpdateChatMessageCommand : IRequest<Result<ChatMessage, ChatException>>
    {
        public required Guid ChatId { get; init; }
        public required Guid MessageId { get; init; }
        public required Guid RequestingUserId { get; init; }
        public required string Content { get; init; }
    }

    public class UpdateChatMessageCommandHandler(
        IChatsRepository chatsRepository,
        IChatMessagesRepository chatMessagesRepository,
        IChatMembersRepository chatMembersRepository
    ) : IRequestHandler<UpdateChatMessageCommand, Result<ChatMessage, ChatException>>
    {
        public async Task<Result<ChatMessage, ChatException>> Handle(UpdateChatMessageCommand request, CancellationToken cancellationToken)
        {
            var chatId = new ChatId(request.ChatId);
            var messageId = new ChatMessageId(request.MessageId);
            var requestingUserId = new UserId(request.RequestingUserId);

            try
            {
                var chatOption = await chatsRepository.GetById(chatId, cancellationToken);
                if (!chatOption.HasValue)
                {
                    return new ChatNotFoundException(chatId);
                }

                var messageOption = await chatMessagesRepository.GetById(messageId, cancellationToken);
                if (!messageOption.HasValue)
                {
                    return new ChatMessageNotFoundException(messageId);
                }

                var message = messageOption.ValueOrFailure();
                if (message.ChatId != chatId)
                {
                    return new ChatMessageOperationFailedException(messageId, "UpdateChatMessage", new InvalidOperationException("Message does not belong to the specified chat."));
                }

                var memberOption = await chatMembersRepository.GetByChatAndUser(chatId, requestingUserId, cancellationToken);
                if (!memberOption.HasValue)
                {
                    return new UserNotMemberException(requestingUserId, chatId);
                }

                if (message.SenderId != requestingUserId)
                {
                    return new InsufficientPermissionsException(requestingUserId, chatId, "edit this message");
                }

                message.EditContent(request.Content);
                var updated = await chatMessagesRepository.Update(message, cancellationToken);
                return updated;
            }
            catch (Exception ex)
            {
                return new ChatMessageOperationFailedException(messageId, "UpdateChatMessage", ex);
            }
        }
    }
}
