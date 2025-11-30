using Microsoft.AspNetCore.SignalR;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Queries;
using Domain.Chats;
using Domain.Users;
using Api.Hubs;
using Api.Dtos;

namespace Api.Services
{
    public class ChatNotificationService : IChatNotificationService
    {
        private readonly IHubContext<ChatHub, IChatClient> _hubContext;
        private readonly IChatsQueries _chatsQueries;
        private readonly ILogger<ChatNotificationService> _logger;

        public ChatNotificationService(
            IHubContext<ChatHub, IChatClient> hubContext,
            IChatsQueries chatsQueries,
            ILogger<ChatNotificationService> logger)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _chatsQueries = chatsQueries ?? throw new ArgumentNullException(nameof(chatsQueries));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task NotifyNewMessage(ChatMessage message, CancellationToken cancellationToken = default)
        {
            try
            {
                // Отримуємо повідомлення з повною інформацією про відправника
                var messages = await _chatsQueries.GetChatMessages(message.ChatId, 0, 1, cancellationToken);
                var fullMessage = messages.FirstOrDefault(m => m.Id == message.Id);

                if (fullMessage == null)
                {
                    _logger.LogWarning("Message {MessageId} not found for notification", message.Id.Value);
                    return;
                }

                var messageDto = ChatMessageDto.FromDomainModel(fullMessage);
                var groupName = GetChatGroupName(message.ChatId);

                await _hubContext.Clients
                    .Group(groupName)
                    .ReceiveMessage(messageDto);

                _logger.LogInformation("Notified chat {ChatId} about new message {MessageId}", 
                    message.ChatId.Value, message.Id.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying about new message {MessageId} in chat {ChatId}", 
                    message.Id.Value, message.ChatId.Value);
            }
        }

        public async Task NotifyMessageEdited(ChatMessage message, CancellationToken cancellationToken = default)
        {
            try
            {
                var messages = await _chatsQueries.GetChatMessages(message.ChatId, 0, 1, cancellationToken);
                var fullMessage = messages.FirstOrDefault(m => m.Id == message.Id);

                if (fullMessage == null)
                {
                    _logger.LogWarning("Message {MessageId} not found for edit notification", message.Id.Value);
                    return;
                }

                var messageDto = ChatMessageDto.FromDomainModel(fullMessage);
                var groupName = GetChatGroupName(message.ChatId);

                await _hubContext.Clients
                    .Group(groupName)
                    .MessageEdited(messageDto);

                _logger.LogInformation("Notified chat {ChatId} about edited message {MessageId}", 
                    message.ChatId.Value, message.Id.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying about edited message {MessageId} in chat {ChatId}", 
                    message.Id.Value, message.ChatId.Value);
            }
        }

        public async Task NotifyMessageDeleted(ChatId chatId, ChatMessageId messageId, CancellationToken cancellationToken = default)
        {
            try
            {
                var groupName = GetChatGroupName(chatId);

                await _hubContext.Clients
                    .Group(groupName)
                    .MessageDeleted(chatId.Value, messageId.Value);

                _logger.LogInformation("Notified chat {ChatId} about deleted message {MessageId}", 
                    chatId.Value, messageId.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying about deleted message {MessageId} in chat {ChatId}", 
                    messageId.Value, chatId.Value);
            }
        }

        public async Task NotifyUserJoined(ChatMember member, CancellationToken cancellationToken = default)
        {
            try
            {
                var members = await _chatsQueries.GetChatMembers(member.ChatId, cancellationToken);
                var fullMember = members.FirstOrDefault(m => m.Id == member.Id);

                if (fullMember == null)
                {
                    _logger.LogWarning("Member {MemberId} not found for join notification", member.Id.Value);
                    return;
                }

                var memberDto = ChatMemberDto.FromDomainModel(fullMember);
                var groupName = GetChatGroupName(member.ChatId);

                await _hubContext.Clients
                    .Group(groupName)
                    .UserJoined(member.ChatId.Value, memberDto);

                _logger.LogInformation("Notified chat {ChatId} about user {UserId} joining", 
                    member.ChatId.Value, member.UserId.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying about user {UserId} joining chat {ChatId}", 
                    member.UserId.Value, member.ChatId.Value);
            }
        }

        public async Task NotifyUserLeft(ChatId chatId, UserId userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var groupName = GetChatGroupName(chatId);

                await _hubContext.Clients
                    .Group(groupName)
                    .UserLeft(chatId.Value, userId.Value);

                _logger.LogInformation("Notified chat {ChatId} about user {UserId} leaving", 
                    chatId.Value, userId.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying about user {UserId} leaving chat {ChatId}", 
                    userId.Value, chatId.Value);
            }
        }

        private static string GetChatGroupName(ChatId chatId) => $"chat_{chatId.Value}";
    }
}
