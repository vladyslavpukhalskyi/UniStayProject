using Application.Common;
using Application.Common.Interfaces.Repositories;
using Application.Chats.Exceptions;
using Domain.Chats;
using Domain.Users;
using MediatR;

namespace Application.Chats.Commands
{
    public record DeleteChatCommand : IRequest<Result<Chat, ChatException>>
    {
        public required Guid ChatId { get; init; }
        public required Guid UserId { get; init; }
    }

    public class DeleteChatCommandHandler(
        IChatsRepository chatsRepository,
        IChatMembersRepository chatMembersRepository
        )
        : IRequestHandler<DeleteChatCommand, Result<Chat, ChatException>>
    {
        public async Task<Result<Chat, ChatException>> Handle(DeleteChatCommand request, CancellationToken cancellationToken)
        {
            var chatId = new ChatId(request.ChatId);
            var userId = new UserId(request.UserId);

            try
            {
                var chatOption = await chatsRepository.GetById(chatId, cancellationToken);

                return await chatOption.Match<Task<Result<Chat, ChatException>>>(
                    some: async chat =>
                    {
                        var memberOption = await chatMembersRepository.GetByChatAndUser(chatId, userId, cancellationToken);

                        return await memberOption.Match<Task<Result<Chat, ChatException>>>(
                            some: async member =>
                            {
                                if (member.Role != ChatEnums.ChatMemberRole.Owner)
                                {
                                    return new InsufficientPermissionsException(userId, chatId, "delete chat");
                                }

                                chat.Deactivate();
                                var updatedChat = await chatsRepository.Update(chat, cancellationToken);
                                return updatedChat;
                            },
                            none: () => Task.FromResult<Result<Chat, ChatException>>(new UserNotMemberException(userId, chatId))
                        );
                    },
                    none: () => Task.FromResult<Result<Chat, ChatException>>(new ChatNotFoundException(chatId))
                );
            }
            catch (Exception exception)
            {
                return new ChatOperationFailedException(chatId, "DeleteChat", exception);
            }
        }
    }
}
