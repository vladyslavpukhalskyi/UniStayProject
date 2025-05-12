// Файл: Application/Reviews/Commands/DeleteReviewCommand.cs
using Application.Common; // Для Result
using Application.Common.Interfaces.Repositories; // Для IReviewsRepository
using Application.Reviews.Exceptions; // Для ReviewException та підтипів
using Domain.Reviews; // Для Review, ReviewId
using Domain.Users;   // Для UserId
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Reviews.Commands
{
    /// <summary>
    /// Команда для видалення відгуку.
    /// </summary>
    public record DeleteReviewCommand : IRequest<Result<Review, ReviewException>>
    {
        /// <summary>
        /// ID відгуку, який потрібно видалити.
        /// </summary>
        public required Guid ReviewId { get; init; }

        /// <summary>
        /// ID користувача, який запитує видалення (для перевірки авторизації).
        /// Встановлюється з контексту аутентифікації.
        /// </summary>
        public required Guid RequestingUserId { get; init; }
    }

    /// <summary>
    /// Обробник команди DeleteReviewCommand.
    /// </summary>
    public class DeleteReviewCommandHandler(
        IReviewsRepository reviewsRepository)
        : IRequestHandler<DeleteReviewCommand, Result<Review, ReviewException>>
    {
        public async Task<Result<Review, ReviewException>> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
        {
            var reviewIdToDelete = new ReviewId(request.ReviewId);
            var requestingUserId = new UserId(request.RequestingUserId);

            // 1. Отримати відгук за ID
            var existingReviewOption = await reviewsRepository.GetById(reviewIdToDelete, cancellationToken);

            return await existingReviewOption.Match<Task<Result<Review, ReviewException>>>(
                some: async review => // Якщо відгук знайдено
                {
                    // 2. Перевірка авторизації: чи є поточний користувач автором відгуку
                    // (Для адміністраторських прав потрібна була б додаткова логіка/перевірка ролі)
                    if (review.UserId != requestingUserId)
                    {
                        return new UserNotAuthorizedForReviewOperationException(requestingUserId, reviewIdToDelete, "DeleteReview");
                    }

                    // 3. Видалити сутність відгуку
                    return await DeleteReviewEntity(review, cancellationToken);
                },
                none: () => // Якщо відгук не знайдено
                {
                    ReviewException exception = new ReviewNotFoundException(reviewIdToDelete);
                    return Task.FromResult<Result<Review, ReviewException>>(exception);
                }
            );
        }

        private async Task<Result<Review, ReviewException>> DeleteReviewEntity(Review review, CancellationToken cancellationToken)
        {
            try
            {
                var deletedReview = await reviewsRepository.Delete(review, cancellationToken);
                return deletedReview; // Implicit conversion
            }
            catch (Exception exception)
            {
                return new ReviewOperationFailedException(review.Id, "DeleteReview", exception);
            }
        }
    }
}