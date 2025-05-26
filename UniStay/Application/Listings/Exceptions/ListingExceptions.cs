using Domain.Listings; 
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Users;

namespace Application.Listings.Exceptions
{
    public abstract class ListingException : Exception
    {
        public ListingId ListingId { get; }

        protected ListingException(ListingId id, string message, Exception? innerException = null)
            : base(message, innerException)
        {
            ListingId = id ?? ListingId.Empty();
        }
    }

    public class ListingNotFoundException : ListingException
    {
        public ListingNotFoundException(ListingId id)
            : base(id, $"Listing with id: {id} not found") { }
    }

    public class InvalidAmenitiesProvidedException : ListingException
    {
        public IEnumerable<Guid> InvalidAmenityIds { get; }
        public InvalidAmenitiesProvidedException(IEnumerable<Guid> invalidAmenityIds)
            : base(ListingId.Empty(), $"Invalid amenity IDs provided: {string.Join(", ", invalidAmenityIds)}")
        {
            InvalidAmenityIds = invalidAmenityIds;
        }
    }

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