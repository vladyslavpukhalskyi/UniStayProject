using Domain.Users;
using System;

namespace Application.Users.Exceptions
{
    public enum UserExceptionType
    {
        None = 0,
        NotFound,
        AlreadyExists,
        InvalidOperation,
        ValidationError,
        Forbidden,
        Unauthorized,
        ExternalServiceError,
        Unknown
    }

    public class UserException : Exception
    {
        public UserExceptionType Type { get; }
        public UserId? UserId { get; }

        public UserException(string message, UserExceptionType type = UserExceptionType.Unknown, Exception? innerException = null)
            : base(message, innerException)
        {
            Type = type;
            UserId = null;
        }

        public UserException(UserId id, string message, UserExceptionType type = UserExceptionType.Unknown, Exception? innerException = null)
            : base(message, innerException)
        {
            Type = type;
            UserId = id;
        }

        public static UserException NotFound(string message = "User not found.") =>
            new UserException(message, UserExceptionType.NotFound);

        public static UserException NotFound(UserId id, string message = null) =>
            new UserException(id, message ?? $"User with id: {id.Value} not found.", UserExceptionType.NotFound);

        public static UserException AlreadyExists(string email, UserId? existingId = null) =>
            new UserException(existingId, $"User with email: {email} already exists.", UserExceptionType.AlreadyExists);

        public static UserException InvalidCredentials(string message = "Invalid credentials (email or password).") =>
            new UserException(message, UserExceptionType.InvalidOperation);

        public static UserException OperationFailed(string message, Exception innerException = null) =>
            new UserException(message, UserExceptionType.InvalidOperation, innerException);

        public static UserException OperationFailed(UserId id, string operation, Exception innerException = null) =>
            new UserException(id, $"Operation '{operation}' failed for user with id: {id.Value}", UserExceptionType.InvalidOperation, innerException);
    }

    public class UserNotFoundException : UserException
    {
        public UserNotFoundException(UserId id) : base(id, $"User with id: {id.Value} not found.", UserExceptionType.NotFound) { }
        public UserNotFoundException(string email) : base(UserId.Empty(), $"User with email: {email} not found.", UserExceptionType.NotFound) { }
        public UserNotFoundException() : base("User not found.", UserExceptionType.NotFound) { }
    }

    public class UserAlreadyExistsException : UserException
    {
        public UserAlreadyExistsException(string email, UserId? existingId = null) : base(existingId, $"User with email: {email} already exists.", UserExceptionType.AlreadyExists) { }
    }

    public class UserOperationFailedException : UserException
    {
        public UserOperationFailedException(UserId id, string operation, Exception innerException) : base(id, $"Operation '{operation}' failed for user with id: {id.Value}", UserExceptionType.InvalidOperation, innerException) { }
        public UserOperationFailedException(string message, Exception innerException) : base(message, UserExceptionType.InvalidOperation, innerException) { }
    }
}