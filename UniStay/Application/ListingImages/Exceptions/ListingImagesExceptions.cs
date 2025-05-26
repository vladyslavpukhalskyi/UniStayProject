using Domain.ListingImages; 
using Domain.Listings;    
using Domain.Users;       
using System;

namespace Application.ListingImages.Exceptions
{
    public abstract class ListingImageException : Exception
    {
        public ListingImageId ListingImageId { get; }

        protected ListingImageException(ListingImageId id, string message, Exception? innerException = null)
            : base(message, innerException)
        {
            ListingImageId = id ?? ListingImageId.Empty();
        }
    }

    public class ListingImageNotFoundException : ListingImageException
    {
        public ListingImageNotFoundException(ListingImageId id)
            : base(id, $"Listing image with id: {id} not found") { }
    }

    public class ListingNotFoundForImageException : ListingImageException
    {
        public ListingId ListingId { get; }
        public ListingNotFoundForImageException(ListingId listingId)
            : base(ListingImageId.Empty(), $"Listing with id: {listingId} not found to associate an image with.")
        {
            ListingId = listingId;
        }
    }

    public class UserNotAuthorizedToManageListingImagesException : ListingImageException
    {
        public UserId AttemptingUserId { get; }
        public ListingId ListingId { get; }
        public UserNotAuthorizedToManageListingImagesException(UserId attemptingUserId, ListingId listingId)
            : base(ListingImageId.Empty(), $"User {attemptingUserId} is not authorized to manage images for listing {listingId}.")
        {
            AttemptingUserId = attemptingUserId;
            ListingId = listingId;
        }
    }
    
    public class ListingImageOperationFailedException : ListingImageException
    {
        public ListingImageOperationFailedException(ListingImageId id, string operation, Exception innerException)
            : base(id, $"Operation '{operation}' failed for listing image with id: {(id == ListingImageId.Empty() ? "N/A" : id.ToString())}", innerException) { }
    }
}