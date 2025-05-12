// Файл: Application/Users/Exceptions/UserExceptions.cs
using Domain.Users; // Потрібен для UserId
using System;

namespace Application.Users.Exceptions
{
    /// <summary>
    /// Базовий клас для винятків, пов'язаних з користувачами.
    /// </summary>
    public abstract class UserException : Exception
    {
        public UserId UserId { get; }

        protected UserException(UserId id, string message, Exception? innerException = null)
            : base(message, innerException)
        {
            // Якщо ID не встановлено (напр., при помилці створення), використовуємо Empty
            UserId = id ?? UserId.Empty();
        }
    }

    /// <summary>
    /// Виняток: користувача не знайдено.
    /// </summary>
    public class UserNotFoundException : UserException
    {
        public UserNotFoundException(UserId id)
            : base(id, $"User with id: {id} not found") { }

        // Можна додати конструктор для пошуку за email
        public UserNotFoundException(string email)
            : base(UserId.Empty(), $"User with email: {email} not found") { }
    }

    /// <summary>
    /// Виняток: користувач з таким email вже існує.
    /// </summary>
    public class UserAlreadyExistsException : UserException
    {
        public UserAlreadyExistsException(string email, UserId? existingId = null)
            : base(existingId ?? UserId.Empty(), $"User with email: {email} already exists.") { }
    }

    /// <summary>
    /// Виняток: невідома помилка під час операції з користувачем.
    /// </summary>
    public class UserOperationFailedException : UserException
    {
        public UserOperationFailedException(UserId id, string operation, Exception innerException)
            : base(id, $"Operation '{operation}' failed for user with id: {id}", innerException) { }
    }
}