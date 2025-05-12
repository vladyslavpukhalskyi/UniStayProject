// Файл: Application/Messages/Exceptions/MessageExceptions.cs
using Domain.Messages; // Для MessageId
using Domain.Users;   // Для UserId
using System;

namespace Application.Messages.Exceptions
{
    /// <summary>
    /// Базовий клас для винятків, пов'язаних з повідомленнями.
    /// </summary>
    public abstract class MessageException : Exception
    {
        public MessageId MessageId { get; }

        protected MessageException(MessageId id, string message, Exception? innerException = null)
            : base(message, innerException)
        {
            MessageId = id ?? MessageId.Empty();
        }
    }

    /// <summary>
    /// Виняток: повідомлення не знайдено.
    /// </summary>
    public class MessageNotFoundException : MessageException
    {
        public MessageNotFoundException(MessageId id)
            : base(id, $"Message with id: {id} not found") { }
    }

    /// <summary>
    /// Виняток: отримувач повідомлення не знайдений.
    /// </summary>
    public class ReceiverNotFoundException : MessageException
    {
        public UserId ReceiverId { get; }
        public ReceiverNotFoundException(UserId receiverId)
            : base(MessageId.Empty(), $"Receiver user with id: {receiverId} not found.")
        {
            ReceiverId = receiverId;
        }
    }

    /// <summary>
    /// Виняток: не можна відправити повідомлення самому собі.
    /// </summary>
    public class CannotSendMessageToSelfException : MessageException
    {
         public UserId UserId { get; }
         public CannotSendMessageToSelfException(UserId userId)
            : base(MessageId.Empty(), $"User {userId} cannot send a message to themselves.")
         {
            UserId = userId;
         }
    }

    /// <summary>
    /// Виняток: помилка під час виконання операції з повідомленням.
    /// </summary>
    public class MessageOperationFailedException : MessageException
    {
        public MessageOperationFailedException(MessageId id, string operation, Exception innerException)
            : base(id, $"Operation '{operation}' failed for message with id: {(id == MessageId.Empty() ? "N/A" : id.ToString())}", innerException) { }
    }
    
    // Файл: Application/Messages/Exceptions/MessageExceptions.cs
// ... (попередні винятки) ...

    /// <summary>
    /// Виняток: користувач не авторизований для виконання операції з повідомленням.
    /// </summary>
    public class UserNotAuthorizedForMessageOperationException : MessageException
    {
        public UserId AttemptingUserId { get; }
        public string Operation {get; }

        public UserNotAuthorizedForMessageOperationException(UserId attemptingUserId, MessageId messageId, string operation)
            : base(messageId, $"User {attemptingUserId} is not authorized to perform operation '{operation}' on message {messageId}.")
        {
            AttemptingUserId = attemptingUserId;
            Operation = operation;
        }
    }
}