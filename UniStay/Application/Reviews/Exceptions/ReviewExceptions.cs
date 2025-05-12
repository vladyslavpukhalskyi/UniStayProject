// Файл: Application/Reviews/Exceptions/ReviewExceptions.cs
using Domain.Listings; // Для ListingId
using Domain.Reviews; // Для ReviewId
using Domain.Users;   // Для UserId
using System;

namespace Application.Reviews.Exceptions
{
    /// <summary>
    /// Базовий клас для винятків, пов'язаних з відгуками.
    /// </summary>
    public abstract class ReviewException : Exception
    {
        public ReviewId ReviewId { get; }

        protected ReviewException(ReviewId id, string message, Exception? innerException = null)
            : base(message, innerException)
        {
            ReviewId = id ?? ReviewId.Empty();
        }
    }

    /// <summary>
    /// Виняток: відгук не знайдено.
    /// </summary>
    public class ReviewNotFoundException : ReviewException
    {
        public ReviewNotFoundException(ReviewId id)
            : base(id, $"Review with id: {id} not found") { }
    }

    /// <summary>
    /// Виняток: оголошення не знайдено для створення/прив'язки відгуку.
    /// </summary>
    public class ListingNotFoundForReviewException : ReviewException
    {
        public ListingId ListingId { get; }
        public ListingNotFoundForReviewException(ListingId listingId)
            : base(ReviewId.Empty(), $"Listing with id: {listingId} not found to associate with a review.")
        {
            ListingId = listingId;
        }
    }

    /// <summary>
    /// Виняток: користувач не може залишити відгук (наприклад, вже залишив або це його оголошення).
    /// </summary>
    public class UserCannotReviewException : ReviewException
    {
        public UserId UserId { get; }
        public ListingId ListingId { get; }
        public UserCannotReviewException(UserId userId, ListingId listingId, string reason)
            : base(ReviewId.Empty(), $"User {userId} cannot review listing {listingId}. Reason: {reason}")
        {
            UserId = userId;
            ListingId = listingId;
        }
    }
    
    /// <summary>
    /// Виняток: помилка під час виконання операції з відгуком.
    /// </summary>
    public class ReviewOperationFailedException : ReviewException
    {
        public ReviewOperationFailedException(ReviewId id, string operation, Exception innerException)
            : base(id, $"Operation '{operation}' failed for review with id: {(id == ReviewId.Empty() ? "N/A" : id.ToString())}", innerException) { }
    }
    // Файл: Application/Reviews/Exceptions/ReviewExceptions.cs
// ... (попередні винятки) ...

    /// <summary>
    /// Виняток: користувач не авторизований для виконання операції з відгуком.
    /// </summary>
    public class UserNotAuthorizedForReviewOperationException : ReviewException
    {
        public UserId AttemptingUserId { get; }
        public string Operation {get; }

        public UserNotAuthorizedForReviewOperationException(UserId attemptingUserId, ReviewId reviewId, string operation)
            : base(reviewId, $"User {attemptingUserId} is not authorized to perform operation '{operation}' on review {reviewId}.")
        {
            AttemptingUserId = attemptingUserId;
            Operation = operation;
        }
    }
}