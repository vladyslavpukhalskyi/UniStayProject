using Domain.Amenities; 
using System;

namespace Application.Amenities.Exceptions
{
    public abstract class AmenityException : Exception
    {
        public AmenityId AmenityId { get; }

        protected AmenityException(AmenityId id, string message, Exception? innerException = null)
            : base(message, innerException)
        {
            AmenityId = id ?? AmenityId.Empty();
        }
    }

    public class AmenityNotFoundException : AmenityException
    {
        public AmenityNotFoundException(AmenityId id)
            : base(id, $"Amenity with id: {id} not found") { }

        public AmenityNotFoundException(string title)
            : base(AmenityId.Empty(), $"Amenity with title: '{title}' not found") { }
    }

    public class AmenityAlreadyExistsException : AmenityException
    {
        public string Title { get; }
        public AmenityAlreadyExistsException(string title, AmenityId? existingId = null)
            : base(existingId ?? AmenityId.Empty(), $"Amenity with title '{title}' already exists.")
        {
            Title = title;
        }
    }

    public class AmenityOperationFailedException : AmenityException
    {
        public AmenityOperationFailedException(AmenityId id, string operation, Exception innerException)
            : base(id, $"Operation '{operation}' failed for amenity with id: {(id == AmenityId.Empty() ? "N/A" : id.ToString())}", innerException) { }
    }
}