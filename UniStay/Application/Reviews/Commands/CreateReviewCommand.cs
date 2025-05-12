// Файл: Application/Reviews/Commands/CreateReviewCommand.cs
using Application.Common; // Для Result
using Application.Common.Interfaces.Repositories; // Для IReviewsRepository, IListingsRepository
using Application.Reviews.Exceptions; // Для ReviewException та підтипів
using Domain.Listings; // Для ListingId
using Domain.Reviews; // Для Review, ReviewId
using Domain.Users;   // Для UserId
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Reviews.Commands
{
    /// <summary>
    /// Команда для створення нового відгуку.
    /// </summary>
    public record CreateReviewCommand : IRequest<Result<Review, ReviewException>>
    {
        public required Guid ListingId { get; init; }
        public required int Rating { get; init; }
        public required string Comment { get; init; }
        
        /// <summary>
        /// ID користувача, який залишає відгук. Встановлюється з контексту аутентифікації.
        /// </summary>
        public required Guid UserId { get; init; }
    }

    /// <summary>
    /// Обробник команди CreateReviewCommand.
    /// </summary>
    public class CreateReviewCommandHandler(
        IReviewsRepository reviewsRepository,
        IListingsRepository listingsRepository 
        )
        : IRequestHandler<CreateReviewCommand, Result<Review, ReviewException>>
    {
        public async Task<Result<Review, ReviewException>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            var listingId = new ListingId(request.ListingId);
            var userId = new UserId(request.UserId);

            // 1. Перевірити, чи існує оголошення (Listing)
            var listingOption = await listingsRepository.GetById(listingId, cancellationToken);

            // ВИКОРИСТОВУЄМО Match для обробки listingOption
            return await listingOption.Match<Task<Result<Review, ReviewException>>>(
                some: async listing => // Якщо оголошення знайдено (listing - це розпакований Listing)
                {
                    // 2. Бізнес-правило: користувач не може залишати відгук на власне оголошення
                    if (listing.UserId == userId)
                    {
                        return new UserCannotReviewException(userId, listingId, "User cannot review their own listing.");
                    }

                    // 3. (Опціонально) Бізнес-правило: користувач може залишити тільки один відгук на оголошення
                    // Це потребуватиме методу в IReviewsRepository, наприклад:
                    // bool hasReviewed = await reviewsRepository.HasUserReviewedListingAsync(userId, listingId, cancellationToken);
                    // if (hasReviewed)
                    // {
                    //     return new UserCannotReviewException(userId, listingId, "User has already reviewed this listing.");
                    // }
                    
                    var reviewId = ReviewId.New();
                    try
                    {
                        // 4. Створити сутність Review
                        var review = Review.New(
                            id: reviewId,
                            userId: userId,
                            listingId: listingId, // Використовуємо listingId, який вже є типу ListingId
                            rating: request.Rating,
                            comment: request.Comment
                        );

                        // 5. Додати відгук в репозиторій
                        var addedReview = await reviewsRepository.Add(review, cancellationToken);
                        return addedReview; // Implicit conversion
                    }
                    catch (Exception exception)
                    {
                        return new ReviewOperationFailedException(reviewId, "CreateReview", exception);
                    }
                },
                none: () => // Якщо оголошення не знайдено
                {
                    // Повернути помилку ListingNotFoundForReviewException
                    ReviewException exception = new ListingNotFoundForReviewException(listingId);
                    return Task.FromResult<Result<Review, ReviewException>>(exception);
                }
            );
        }
    }
}