using Application.Common;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Chats.Exceptions;
using Domain.Chats;
using Domain.Users;
using MediatR;

namespace Application.Chats.Commands
{
    public record LeaveChatCommand : IRequest<Result<ChatMember, ChatException>>
    {
        public required Guid ChatId { get; init; }
        public required Guid UserId { get; init; }
    }

    public class LeaveChatCommandHandler(
        IChatsRepository chatsRepository,
        IChatMembersRepository chatMembersRepository,
        IChatNotificationService chatNotificationService
        )
        : IRequestHandler<LeaveChatCommand, Result<ChatMember, ChatException>>
    {
        public async Task<Result<ChatMember, ChatException>> Handle(LeaveChatCommand request, CancellationToken cancellationToken)
        {
            var chatId = new ChatId(request.ChatId);
            var userId = new UserId(request.UserId);

            try
            {
                var chatOption = await chatsRepository.GetById(chatId, cancellationToken);
                if (!chatOption.HasValue)
                {
                    return new ChatNotFoundException(chatId);
                }

                var memberOption = await chatMembersRepository.GetByChatAndUser(chatId, userId, cancellationToken);
                return await memberOption.Match<Task<Result<ChatMember, ChatException>>>(
                    some: async member =>
                    {
                        member.Leave();
                        var updatedMember = await chatMembersRepository.Update(member, cancellationToken);
                        
                        // Відправляємо real-time сповіщення через SignalR
                        await chatNotificationService.NotifyUserLeft(chatId, userId, cancellationToken);
                        
                        return updatedMember;
                    },
                    none: () => Task.FromResult<Result<ChatMember, ChatException>>(new UserNotMemberException(userId, chatId))
                );
            }
            catch (Exception exception)
            {
                return new ChatOperationFailedException(chatId, "LeaveChat", exception);
            }
        }
    }
}
