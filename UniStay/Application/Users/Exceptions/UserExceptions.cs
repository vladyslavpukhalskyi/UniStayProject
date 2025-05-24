// Файл: Application/Users/Exceptions/UserExceptions.cs
using Domain.Users; // Потрібен для UserId
using System;

namespace Application.Users.Exceptions
{
    /// <summary>
    /// Визначає типи помилок, пов'язаних з користувачами.
    /// </summary>
    public enum UserExceptionType
    {
        None = 0, // Тип за замовчуванням, якщо не вказано конкретний
        NotFound,
        AlreadyExists,
        InvalidOperation, // Для загальних логічних помилок, таких як невірні облікові дані
        ValidationError,
        Forbidden, // Для помилок авторизації (немає прав)
        Unauthorized, // Для помилок автентифікації (не авторизовано)
        ExternalServiceError, // Якщо помилка приходить від зовнішнього сервісу
        Unknown // Якщо тип помилки невідомий
    }

    /// <summary>
    /// Базовий клас для винятків, пов'язаних з користувачами.
    /// Зроблено неабстрактним, щоб його можна було інстанціювати безпосередньо,
    /// що відповідає потребам UserErrorHandler.
    /// </summary>
    public class UserException : Exception
    {
        public UserExceptionType Type { get; }
        public UserId? UserId { get; } // Тепер nullable, оскільки не завжди може бути відомий ID користувача

        // Основний конструктор для загальних UserException
        public UserException(string message, UserExceptionType type = UserExceptionType.Unknown, Exception? innerException = null)
            : base(message, innerException)
        {
            Type = type;
            UserId = null; // За замовчуванням null, якщо не передано
        }

        // Конструктор, що приймає UserId, якщо ID користувача відомий
        public UserException(UserId id, string message, UserExceptionType type = UserExceptionType.Unknown, Exception? innerException = null)
            : base(message, innerException)
        {
            Type = type;
            UserId = id;
        }

        // --- Фабричні методи для зручності ---

        /// <summary>
        /// Створює виняток для випадку, коли користувача не знайдено.
        /// </summary>
        public static UserException NotFound(string message = "User not found.") =>
            new UserException(message, UserExceptionType.NotFound);

        /// <summary>
        /// Створює виняток для випадку, коли користувача з певним ID не знайдено.
        /// </summary>
        public static UserException NotFound(UserId id, string message = null) =>
            new UserException(id, message ?? $"User with id: {id.Value} not found.", UserExceptionType.NotFound);

        /// <summary>
        /// Створює виняток для випадку, коли користувач з таким email вже існує.
        /// </summary>
        public static UserException AlreadyExists(string email, UserId? existingId = null) =>
            new UserException(existingId, $"User with email: {email} already exists.", UserExceptionType.AlreadyExists);

        /// <summary>
        /// Створює виняток для невірних облікових даних (логін/пароль).
        /// </summary>
        public static UserException InvalidCredentials(string message = "Invalid credentials (email or password).") =>
            new UserException(message, UserExceptionType.InvalidOperation); // Можливо, UserExceptionType.Unauthorized буде кращим

        /// <summary>
        /// Створює виняток для загальних помилок операцій з користувачем.
        /// </summary>
        public static UserException OperationFailed(string message, Exception innerException = null) =>
            new UserException(message, UserExceptionType.InvalidOperation, innerException);

        /// <summary>
        /// Створює виняток для загальних помилок операцій з користувачем за ID.
        /// </summary>
        public static UserException OperationFailed(UserId id, string operation, Exception innerException = null) =>
            new UserException(id, $"Operation '{operation}' failed for user with id: {id.Value}", UserExceptionType.InvalidOperation, innerException);

        // ... Додайте інші фабричні методи за потребою ...
    }

    // Якщо ви хочете зберегти специфічні типи винятків для "типізації"
    // Тоді ці класи успадковують від базового UserException
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