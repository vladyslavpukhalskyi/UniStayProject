// Файл: Application/Reviews/Commands/UpdateReviewCommand.cs
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
    /// Команда для оновлення існуючого відгуку.
    /// </summary>
    public record UpdateReviewCommand : IRequest<Result<Review, ReviewException>>
    {
        /// <summary>
        /// ID відгуку, який оновлюється.
        /// </summary>
        public required Guid ReviewId { get; init; }

        // Поля, які можна оновити
        public required int Rating { get; init; }
        public required string Comment { get; init; }

        /// <summary>
        /// ID користувача, який запитує оновлення (для перевірки авторизації).
        /// Встановлюється з контексту аутентифікації.
        /// </summary>
        public required Guid RequestingUserId { get; init; }
    }

    /// <summary>
    /// Обробник команди UpdateReviewCommand.
    /// </summary>
    public class UpdateReviewCommandHandler(
        IReviewsRepository reviewsRepository)
        : IRequestHandler<UpdateReviewCommand, Result<Review, ReviewException>>
    {
        public async Task<Result<Review, ReviewException>> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
        {
            var reviewIdToUpdate = new ReviewId(request.ReviewId);
            var requestingUserId = new UserId(request.RequestingUserId);

            // 1. Отримати відгук за ID
            var existingReviewOption = await reviewsRepository.GetById(reviewIdToUpdate, cancellationToken);

            return await existingReviewOption.Match<Task<Result<Review, ReviewException>>>(
                some: async review => // Якщо відгук знайдено
                {
                    // 2. Перевірка авторизації: чи є поточний користувач автором відгуку
                    if (review.UserId != requestingUserId)
                    {
                        return new UserNotAuthorizedForReviewOperationException(requestingUserId, reviewIdToUpdate, "UpdateReview");
                    }

                    // 3. Оновити сутність відгуку
                    return await UpdateReviewEntity(review, request, cancellationToken);
                },
                none: () => // Якщо відгук не знайдено
                {
                    ReviewException exception = new ReviewNotFoundException(reviewIdToUpdate);
                    return Task.FromResult<Result<Review, ReviewException>>(exception);
                }
            );
        }

        private async Task<Result<Review, ReviewException>> UpdateReviewEntity(Review review, UpdateReviewCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 4. Оновити дані відгуку за допомогою методу доменної моделі
                review.UpdateReview(request.Rating, request.Comment);

                // 5. Зберегти оновлений відгук через репозиторій
                var updatedReview = await reviewsRepository.Update(review, cancellationToken);
                return updatedReview; // Implicit conversion
            }
            catch (Exception exception)
            {
                // 6. Обробити можливі помилки під час збереження
                return new ReviewOperationFailedException(review.Id, "UpdateReview", exception);
            }
        }
    }
}