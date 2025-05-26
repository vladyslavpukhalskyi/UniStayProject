using Domain.Favorites; 
using Domain.Listings; 
using Domain.Users;   
using System;

namespace Application.Favorites.Exceptions
{
    public abstract class FavoriteException : Exception
    {
        public FavoriteId FavoriteId { get; }

        protected FavoriteException(FavoriteId id, string message, Exception? innerException = null)
            : base(message, innerException)
        {
            FavoriteId = id ?? FavoriteId.Empty();
        }
    }

    public class FavoriteNotFoundException : FavoriteException
    {
        public FavoriteNotFoundException(FavoriteId id)
            : base(id, $"Favorite record with id: {id} not found") { }
    }

    public class ListingNotFoundForFavoriteException : FavoriteException
    {
        public ListingId ListingId { get; }
        public ListingNotFoundForFavoriteException(ListingId listingId)
            : base(FavoriteId.Empty(), $"Listing with id: {listingId} not found to add to favorites.")
        {
            ListingId = listingId;
        }
    }

    public class UserNotFoundForFavoriteException : FavoriteException
    {
        public UserId UserId { get; }
        public UserNotFoundForFavoriteException(UserId userId)
            : base(FavoriteId.Empty(), $"User with id: {userId} not found to add a favorite.")
        {
            UserId = userId;
        }
    }

    public class UserAlreadyFavoritedListingException : FavoriteException
    {
        public UserId UserId { get; }
        public ListingId ListingId { get; }
         public UserAlreadyFavoritedListingException(UserId userId, ListingId listingId)
            : base(FavoriteId.Empty(), $"User {userId} has already favorited listing {listingId}.")
         {
             UserId = userId;
             ListingId = listingId;
         }
    }

    public class FavoriteOperationFailedException : FavoriteException
    {
        public FavoriteOperationFailedException(FavoriteId id, string operation, Exception innerException)
            : base(id, $"Operation '{operation}' failed for favorite record with id: {(id == FavoriteId.Empty() ? "N/A" : id.ToString())}", innerException) { }
    }
    
    public class UserHasNotFavoritedListingException : FavoriteException
    {
        public UserId UserId { get; }
        public ListingId ListingId { get; }
        public UserHasNotFavoritedListingException(UserId userId, ListingId listingId)
            : base(FavoriteId.Empty(), $"User {userId} has not favorited listing {listingId}.")
        {
            UserId = userId;
            ListingId = listingId;
        }
    }
}