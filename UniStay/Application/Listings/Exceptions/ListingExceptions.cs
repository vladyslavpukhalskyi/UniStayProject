// Файл: Application/Listings/Exceptions/ListingExceptions.cs
using Domain.Listings; // Для ListingId
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Users;

namespace Application.Listings.Exceptions
{
    /// <summary>
    /// Базовий клас для винятків, пов'язаних з оголошеннями.
    /// </summary>
    public abstract class ListingException : Exception
    {
        public ListingId ListingId { get; }

        protected ListingException(ListingId id, string message, Exception? innerException = null)
            : base(message, innerException)
        {
            ListingId = id ?? ListingId.Empty();
        }
    }

    /// <summary>
    /// Виняток: оголошення не знайдено.
    /// </summary>
    public class ListingNotFoundException : ListingException
    {
        public ListingNotFoundException(ListingId id)
            : base(id, $"Listing with id: {id} not found") { }
    }

    /// <summary>
    /// Виняток: надано недійсні ID зручностей при створенні/оновленні оголошення.
    /// </summary>
    public class InvalidAmenitiesProvidedException : ListingException
    {
        public IEnumerable<Guid> InvalidAmenityIds { get; }
        public InvalidAmenitiesProvidedException(IEnumerable<Guid> invalidAmenityIds)
            : base(ListingId.Empty(), $"Invalid amenity IDs provided: {string.Join(", ", invalidAmenityIds)}")
        {
            InvalidAmenityIds = invalidAmenityIds;
        }
    }

    /// <summary>
    /// Виняток: помилка під час виконання операції з оголошенням.
    /// </summary>
    public class ListingOperationFailedException : ListingException
    {
        public ListingOperationFailedException(ListingId id, string operation, Exception innerException)
            : base(id, $"Operation '{operation}' failed for listing with id: {(id == ListingId.Empty() ? "N/A" : id.ToString())}", innerException) { }
    }
    
    public class UserNotAuthorizedForListingOperationException : ListingException
    {
        public UserId AttemptingUserId { get; }
        public string Operation {get; }

        public UserNotAuthorizedForListingOperationException(UserId attemptingUserId, ListingId listingId, string operation)
            : base(listingId, $"User {attemptingUserId} is not authorized to perform operation '{operation}' on listing {listingId}.")
        {
            AttemptingUserId = attemptingUserId;
            Operation = operation;
        }
    }
}