using Application.Common;
using Application.Common.Interfaces.Repositories;
using Application.Chats.Exceptions;
using Domain.Chats;
using Domain.Users;
using MediatR;

namespace Application.Chats.Commands
{
    public record AddUserToChatCommand : IRequest<Result<ChatMember, ChatException>>
    {
        public required Guid ChatId { get; init; }
        public required Guid RequestorUserId { get; init; }
        public required Guid TargetUserId { get; init; }
        public ChatEnums.ChatMemberRole Role { get; init; } = ChatEnums.ChatMemberRole.Member;
    }

    public class AddUserToChatCommandHandler(
        IChatsRepository chatsRepository,
        IChatMembersRepository chatMembersRepository,
        IUsersRepository usersRepository
        ) : IRequestHandler<AddUserToChatCommand, Result<ChatMember, ChatException>>
    {
        public async Task<Result<ChatMember, ChatException>> Handle(AddUserToChatCommand request, CancellationToken cancellationToken)
        {
            var chatId = new ChatId(request.ChatId);
            var requestorId = new UserId(request.RequestorUserId);
            var targetUserId = new UserId(request.TargetUserId);

            try
            {
                var chatOption = await chatsRepository.GetById(chatId, cancellationToken);
                return await chatOption.Match<Task<Result<ChatMember, ChatException>>>(
                    some: async _ =>
                    {
                        var requestorMemberOption = await chatMembersRepository.GetByChatAndUser(chatId, requestorId, cancellationToken);
                        return await requestorMemberOption.Match<Task<Result<ChatMember, ChatException>>>(
                            some: async requestorMember =>
                            {
                                if (requestorMember.Role is not ChatEnums.ChatMemberRole.Owner and not ChatEnums.ChatMemberRole.Admin)
                                    return new InsufficientPermissionsException(requestorId, chatId, "add members");

                                var userOption = await usersRepository.GetById(targetUserId, cancellationToken);
                                if (!userOption.HasValue)
                                    return new ChatOperationFailedException(chatId, "AddUserToChat", new Exception("Target user not found"));

                                var existingMemberOption = await chatMembersRepository.GetByChatAndUser(chatId, targetUserId, cancellationToken);
                                if (existingMemberOption.HasValue)
                                    return new UserAlreadyMemberException(targetUserId, chatId);

                                var member = ChatMember.New(
                                    id: ChatMemberId.New(),
                                    chatId: chatId,
                                    userId: targetUserId,
                                    role: request.Role
                                );

                                var added = await chatMembersRepository.Add(member, cancellationToken);
                                return added;
                            },
                            none: () => Task.FromResult<Result<ChatMember, ChatException>>(new UserNotMemberException(requestorId, chatId))
                        );
                    },
                    none: () => Task.FromResult<Result<ChatMember, ChatException>>(new ChatNotFoundException(chatId))
                );
            }
            catch (Exception ex)
            {
                return new ChatOperationFailedException(chatId, "AddUserToChat", ex);
            }
        }
    }
}
