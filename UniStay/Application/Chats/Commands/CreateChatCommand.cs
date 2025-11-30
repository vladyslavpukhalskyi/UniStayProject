using Application.Common;
using Application.Common.Interfaces.Repositories;
using Application.Chats.Exceptions;
using Domain.Chats;
using Domain.Users;
using MediatR;

namespace Application.Chats.Commands
{
    public record CreateChatCommand : IRequest<Result<Chat, ChatException>>
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
        public required ChatEnums.ChatType Type { get; init; }
        public required Guid CreatedById { get; init; }
    }

    public class CreateChatCommandHandler(
        IChatsRepository chatsRepository,
        IChatMembersRepository chatMembersRepository,
        IUsersRepository usersRepository
        )
        : IRequestHandler<CreateChatCommand, Result<Chat, ChatException>>
    {
        public async Task<Result<Chat, ChatException>> Handle(CreateChatCommand request, CancellationToken cancellationToken)
        {
            var createdById = new UserId(request.CreatedById);
            var chatId = ChatId.New();

            try
            {
                var userOption = await usersRepository.GetById(createdById, cancellationToken);
                if (!userOption.HasValue)
                {
                    return new ChatOperationFailedException(chatId, "CreateChat", new Exception("User not found"));
                }

                var chat = Chat.New(
                    id: chatId,
                    name: request.Name,
                    description: request.Description,
                    type: request.Type,
                    createdById: createdById
                );

                var addedChat = await chatsRepository.Add(chat, cancellationToken);

                var ownerMember = ChatMember.New(
                    id: ChatMemberId.New(),
                    chatId: chatId,
                    userId: createdById,
                    role: ChatEnums.ChatMemberRole.Owner
                );

                await chatMembersRepository.Add(ownerMember, cancellationToken);

                return addedChat;
            }
            catch (Exception exception)
            {
                return new ChatOperationFailedException(chatId, "CreateChat", exception);
            }
        }
    }
}
