using Application.Common;
using Application.Chats.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Domain.Chats;
using Domain.Users;
using MediatR;
using Optional.Unsafe;

namespace Application.Chats.Commands
{
    public record DeleteChatMessageCommand : IRequest<Result<ChatMessage, ChatException>>
    {
        public required Guid ChatId { get; init; }
        public required Guid MessageId { get; init; }
        public required Guid RequestingUserId { get; init; }
    }

    public class DeleteChatMessageCommandHandler(
        IChatsRepository chatsRepository,
        IChatMessagesRepository chatMessagesRepository,
        IChatMembersRepository chatMembersRepository,
        IChatNotificationService chatNotificationService
    ) : IRequestHandler<DeleteChatMessageCommand, Result<ChatMessage, ChatException>>
    {
        public async Task<Result<ChatMessage, ChatException>> Handle(DeleteChatMessageCommand request, CancellationToken cancellationToken)
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
                    return new ChatMessageOperationFailedException(messageId, "DeleteChatMessage", new InvalidOperationException("Message does not belong to the specified chat."));
                }

                var memberOption = await chatMembersRepository.GetByChatAndUser(chatId, requestingUserId, cancellationToken);
                if (!memberOption.HasValue)
                {
                    return new UserNotMemberException(requestingUserId, chatId);
                }

                if (message.SenderId != requestingUserId)
                {
                    return new InsufficientPermissionsException(requestingUserId, chatId, "delete this message");
                }

                message.Delete();
                var updated = await chatMessagesRepository.Update(message, cancellationToken);
                
                // Відправляємо real-time сповіщення через SignalR
                await chatNotificationService.NotifyMessageDeleted(chatId, messageId, cancellationToken);
                
                return updated;
            }
            catch (Exception ex)
            {
                return new ChatMessageOperationFailedException(messageId, "DeleteChatMessage", ex);
            }
        }
    }
}
