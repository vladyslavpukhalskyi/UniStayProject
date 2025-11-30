using Application.Common;
using Application.Chats.Exceptions;
using Domain.Chats;
using MediatR;
using System;

namespace Application.Chats.Commands
{
    [Obsolete("Invite codes are disabled. Use AddUserToChatCommand instead.")]
    public record JoinChatByInviteCommand : IRequest<Result<Domain.Chats.ChatMember, ChatException>>
    {
        public required string InviteCode { get; init; }
        public required Guid UserId { get; init; }
    }

    [Obsolete("Invite codes are disabled. Use AddUserToChatCommand instead.")]
    public class JoinChatByInviteCommandHandler : IRequestHandler<JoinChatByInviteCommand, Result<Domain.Chats.ChatMember, ChatException>>
    {
        public Task<Result<Domain.Chats.ChatMember, ChatException>> Handle(JoinChatByInviteCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult<Result<Domain.Chats.ChatMember, ChatException>>(
                new ChatOperationFailedException(ChatId.Empty, "JoinChatByInvite", new NotSupportedException("Invite codes are disabled"))
            );
        }
    }
}
