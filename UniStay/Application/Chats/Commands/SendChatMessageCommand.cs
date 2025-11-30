using Application.Common;
using Application.Common.Interfaces.Repositories;
using Application.Chats.Exceptions;
using Domain.Chats;
using Domain.Users;
using MediatR;

namespace Application.Chats.Commands
{
    public record SendChatMessageCommand : IRequest<Result<ChatMessage, ChatException>>
    {
        public required Guid ChatId { get; init; }
        public required Guid SenderId { get; init; }
        public required string Content { get; init; }
    }

    public class SendChatMessageCommandHandler(
        IChatsRepository chatsRepository,
        IChatMessagesRepository chatMessagesRepository,
        IChatMembersRepository chatMembersRepository
        )
        : IRequestHandler<SendChatMessageCommand, Result<ChatMessage, ChatException>>
    {
        public async Task<Result<ChatMessage, ChatException>> Handle(SendChatMessageCommand request, CancellationToken cancellationToken)
        {
            var chatId = new ChatId(request.ChatId);
            var senderId = new UserId(request.SenderId);
            var messageId = ChatMessageId.New();

            try
            {
                var chatOption = await chatsRepository.GetById(chatId, cancellationToken);
                if (!chatOption.HasValue)
                {
                    return new ChatNotFoundException(chatId);
                }

                var memberOption = await chatMembersRepository.GetByChatAndUser(chatId, senderId, cancellationToken);
                if (!memberOption.HasValue)
                {
                    return new UserNotMemberException(senderId, chatId);
                }

                var message = ChatMessage.New(
                    id: messageId,
                    chatId: chatId,
                    senderId: senderId,
                    content: request.Content
                );

                var addedMessage = await chatMessagesRepository.Add(message, cancellationToken);
                return addedMessage;
            }
            catch (Exception exception)
            {
                return new ChatMessageOperationFailedException(messageId, "SendChatMessage", exception);
            }
        }
    }
}
