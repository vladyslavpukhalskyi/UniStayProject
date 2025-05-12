// Файл: Application/Amenities/Exceptions/AmenityExceptions.cs
using Domain.Amenities; // Для AmenityId
using System;

namespace Application.Amenities.Exceptions
{
    /// <summary>
    /// Базовий клас для винятків, пов'язаних зі зручностями.
    /// </summary>
    public abstract class AmenityException : Exception
    {
        public AmenityId AmenityId { get; }

        protected AmenityException(AmenityId id, string message, Exception? innerException = null)
            : base(message, innerException)
        {
            AmenityId = id ?? AmenityId.Empty();
        }
    }

    /// <summary>
    /// Виняток: зручність не знайдено.
    /// </summary>
    public class AmenityNotFoundException : AmenityException
    {
        public AmenityNotFoundException(AmenityId id)
            : base(id, $"Amenity with id: {id} not found") { }

        // Конструктор для пошуку за назвою
        public AmenityNotFoundException(string title)
            : base(AmenityId.Empty(), $"Amenity with title: '{title}' not found") { }
    }

    /// <summary>
    /// Виняток: зручність з такою назвою вже існує.
    /// </summary>
    public class AmenityAlreadyExistsException : AmenityException
    {
        public string Title { get; }
        public AmenityAlreadyExistsException(string title, AmenityId? existingId = null)
            : base(existingId ?? AmenityId.Empty(), $"Amenity with title '{title}' already exists.")
        {
            Title = title;
        }
    }

    /// <summary>
    /// Виняток: помилка під час виконання операції зі зручністю.
    /// </summary>
    public class AmenityOperationFailedException : AmenityException
    {
        public AmenityOperationFailedException(AmenityId id, string operation, Exception innerException)
            : base(id, $"Operation '{operation}' failed for amenity with id: {(id == AmenityId.Empty() ? "N/A" : id.ToString())}", innerException) { }
    }
}