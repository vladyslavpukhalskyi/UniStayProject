// Файл: Application/ListingImages/Exceptions/ListingImageExceptions.cs
using Domain.ListingImages; // Для ListingImageId
using Domain.Listings;    // Для ListingId
using Domain.Users;       // Для UserId
using System;

namespace Application.ListingImages.Exceptions
{
    /// <summary>
    /// Базовий клас для винятків, пов'язаних із зображеннями оголошень.
    /// </summary>
    public abstract class ListingImageException : Exception
    {
        public ListingImageId ListingImageId { get; }

        protected ListingImageException(ListingImageId id, string message, Exception? innerException = null)
            : base(message, innerException)
        {
            ListingImageId = id ?? ListingImageId.Empty();
        }
    }

    /// <summary>
    /// Виняток: зображення оголошення не знайдено.
    /// </summary>
    public class ListingImageNotFoundException : ListingImageException
    {
        public ListingImageNotFoundException(ListingImageId id)
            : base(id, $"Listing image with id: {id} not found") { }
    }

    /// <summary>
    /// Виняток: оголошення не знайдено для додавання/зв'язування зображення.
    /// </summary>
    public class ListingNotFoundForImageException : ListingImageException
    {
        public ListingId ListingId { get; }
        public ListingNotFoundForImageException(ListingId listingId)
            : base(ListingImageId.Empty(), $"Listing with id: {listingId} not found to associate an image with.")
        {
            ListingId = listingId;
        }
    }

    /// <summary>
    /// Виняток: користувач не авторизований для керування зображеннями цього оголошення.
    /// </summary>
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
    
    /// <summary>
    /// Виняток: помилка під час виконання операції із зображенням оголошення.
    /// </summary>
    public class ListingImageOperationFailedException : ListingImageException
    {
        public ListingImageOperationFailedException(ListingImageId id, string operation, Exception innerException)
            : base(id, $"Operation '{operation}' failed for listing image with id: {(id == ListingImageId.Empty() ? "N/A" : id.ToString())}", innerException) { }
    }
}