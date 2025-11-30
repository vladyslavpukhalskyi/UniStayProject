using Domain.Chats;
using Domain.Users;

namespace Application.Chats.Exceptions
{
    public abstract class ChatException : Exception
    {
        protected ChatException(string message) : base(message) { }
        protected ChatException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class ChatNotFoundException : ChatException
    {
        public ChatNotFoundException(ChatId chatId) : base($"Chat with ID {chatId} was not found.") { }
    }

    public class UserNotMemberException : ChatException
    {
        public UserNotMemberException(UserId userId, ChatId chatId) : base($"User {userId} is not a member of chat {chatId}.") { }
    }

    public class UserAlreadyMemberException : ChatException
    {
        public UserAlreadyMemberException(UserId userId, ChatId chatId) : base($"User {userId} is already a member of chat {chatId}.") { }
    }

    public class InsufficientPermissionsException : ChatException
    {
        public InsufficientPermissionsException(UserId userId, ChatId chatId, string action) : base($"User {userId} does not have permission to {action} in chat {chatId}.") { }
    }

    public class ChatOperationFailedException : ChatException
    {
        public ChatOperationFailedException(ChatId chatId, string operation, Exception innerException) 
            : base($"Failed to {operation} for chat {chatId}.", innerException) { }
    }

    public class ChatMessageNotFoundException : ChatException
    {
        public ChatMessageNotFoundException(ChatMessageId messageId) : base($"Chat message with ID {messageId} was not found.") { }
    }

    public class ChatMessageOperationFailedException : ChatException
    {
        public ChatMessageOperationFailedException(ChatMessageId messageId, string operation, Exception innerException) 
            : base($"Failed to {operation} for chat message {messageId}.", innerException) { }
    }

    public class ChatMemberNotFoundException : ChatException
    {
        public ChatMemberNotFoundException(ChatMemberId memberId) : base($"Chat member with ID {memberId} was not found.") { }
    }
}
