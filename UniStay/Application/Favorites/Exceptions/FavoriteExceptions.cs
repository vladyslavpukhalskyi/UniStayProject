// Файл: Application/Favorites/Exceptions/FavoriteExceptions.cs
using Domain.Favorites; // Для FavoriteId
using Domain.Listings; // Для ListingId
using Domain.Users;   // Для UserId
using System;

namespace Application.Favorites.Exceptions
{
    /// <summary>
    /// Базовий клас для винятків, пов'язаних з обраним.
    /// </summary>
    public abstract class FavoriteException : Exception
    {
        // Може бути порожнім, якщо помилка стосується ListingId або UserId
        public FavoriteId FavoriteId { get; }

        protected FavoriteException(FavoriteId id, string message, Exception? innerException = null)
            : base(message, innerException)
        {
            FavoriteId = id ?? FavoriteId.Empty();
        }
    }

    /// <summary>
    /// Виняток: запис Favorite не знайдено (можливо, менш актуально для Create).
    /// </summary>
    public class FavoriteNotFoundException : FavoriteException
    {
        public FavoriteNotFoundException(FavoriteId id)
            : base(id, $"Favorite record with id: {id} not found") { }
    }

     /// <summary>
    /// Виняток: оголошення не знайдено для додавання в обране.
    /// </summary>
    public class ListingNotFoundForFavoriteException : FavoriteException
    {
        public ListingId ListingId { get; }
        public ListingNotFoundForFavoriteException(ListingId listingId)
            : base(FavoriteId.Empty(), $"Listing with id: {listingId} not found to add to favorites.")
        {
            ListingId = listingId;
        }
    }

     /// <summary>
    /// Виняток: користувач не знайдений для додавання в обране.
    /// </summary>
    public class UserNotFoundForFavoriteException : FavoriteException
    {
        public UserId UserId { get; }
        public UserNotFoundForFavoriteException(UserId userId)
            : base(FavoriteId.Empty(), $"User with id: {userId} not found to add a favorite.")
        {
            UserId = userId;
        }
    }

    /// <summary>
    /// Виняток: користувач вже додав це оголошення в обране.
    /// </summary>
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

    /// <summary>
    /// Виняток: помилка під час виконання операції з обраним.
    /// </summary>
    public class FavoriteOperationFailedException : FavoriteException
    {
        public FavoriteOperationFailedException(FavoriteId id, string operation, Exception innerException)
            : base(id, $"Operation '{operation}' failed for favorite record with id: {(id == FavoriteId.Empty() ? "N/A" : id.ToString())}", innerException) { }
    }
    
    // Файл: Application/Favorites/Exceptions/FavoriteExceptions.cs
// ... (попередні винятки) ...

    /// <summary>
    /// Виняток: користувач не додавав це оголошення в обране.
    /// </summary>
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