using Domain.Messages; 
using Domain.Users;   
using System;

namespace Application.Messages.Exceptions
{
    public abstract class MessageException : Exception
    {
        public MessageId MessageId { get; }

        protected MessageException(MessageId id, string message, Exception? innerException = null)
            : base(message, innerException)
        {
            MessageId = id ?? MessageId.Empty();
        }
    }

    public class MessageNotFoundException : MessageException
    {
        public MessageNotFoundException(MessageId id)
            : base(id, $"Message with id: {id} not found") { }
    }

    public class ReceiverNotFoundException : MessageException
    {
        public UserId ReceiverId { get; }
        public ReceiverNotFoundException(UserId receiverId)
            : base(MessageId.Empty(), $"Receiver user with id: {receiverId} not found.")
        {
            ReceiverId = receiverId;
        }
    }

    public class CannotSendMessageToSelfException : MessageException
    {
         public UserId UserId { get; }
         public CannotSendMessageToSelfException(UserId userId)
            : base(MessageId.Empty(), $"User {userId} cannot send a message to themselves.")
         {
            UserId = userId;
         }
    }

    public class MessageOperationFailedException : MessageException
    {
        public MessageOperationFailedException(MessageId id, string operation, Exception innerException)
            : base(id, $"Operation '{operation}' failed for message with id: {(id == MessageId.Empty() ? "N/A" : id.ToString())}", innerException) { }
    }
    
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