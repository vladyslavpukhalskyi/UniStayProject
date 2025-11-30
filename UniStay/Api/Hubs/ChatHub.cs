using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Api.Dtos;
using Application.Common.Interfaces.Queries;
using Domain.Chats;
using Domain.Users;

namespace Api.Hubs
{
    [Authorize]
    public class ChatHub : Hub<IChatClient>
    {
        private readonly IChatsQueries _chatsQueries;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IChatsQueries chatsQueries, ILogger<ChatHub> logger)
        {
            _chatsQueries = chatsQueries ?? throw new ArgumentNullException(nameof(chatsQueries));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Підключення до чату
        /// </summary>
        public async Task JoinChat(Guid chatId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized user tried to join chat {ChatId}", chatId);
                return;
            }

            var chatIdDomain = new ChatId(chatId);
            var userIdDomain = new UserId(userId.Value);

            // Перевіряємо чи користувач є членом чату
            var isMember = await _chatsQueries.IsUserMember(chatIdDomain, userIdDomain, Context.ConnectionAborted);
            if (!isMember)
            {
                _logger.LogWarning("User {UserId} is not a member of chat {ChatId}", userId, chatId);
                throw new HubException("You are not a member of this chat");
            }

            // Додаємо користувача до групи чату
            await Groups.AddToGroupAsync(Context.ConnectionId, GetChatGroupName(chatId));
            
            _logger.LogInformation("User {UserId} joined chat {ChatId} with connection {ConnectionId}", 
                userId, chatId, Context.ConnectionId);
        }

        /// <summary>
        /// Відключення від чату
        /// </summary>
        public async Task LeaveChat(Guid chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetChatGroupName(chatId));
            
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} left chat {ChatId} with connection {ConnectionId}", 
                userId, chatId, Context.ConnectionId);
        }

        /// <summary>
        /// Сповіщення про те, що користувач друкує
        /// </summary>
        public async Task NotifyTyping(Guid chatId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return;

            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

            await Clients
                .OthersInGroup(GetChatGroupName(chatId))
                .UserTyping(chatId, userId.Value, userName);
        }

        /// <summary>
        /// Сповіщення про те, що користувач перестав друкувати
        /// </summary>
        public async Task NotifyStoppedTyping(Guid chatId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return;

            await Clients
                .OthersInGroup(GetChatGroupName(chatId))
                .UserStoppedTyping(chatId, userId.Value);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", 
                userId, Context.ConnectionId);
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} disconnected with connection {ConnectionId}. Exception: {Exception}", 
                userId, Context.ConnectionId, exception?.Message);
            
            await base.OnDisconnectedAsync(exception);
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private static string GetChatGroupName(Guid chatId) => $"chat_{chatId}";
    }
}
