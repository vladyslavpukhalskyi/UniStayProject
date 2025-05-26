using Application.Common; 
using Application.Common.Interfaces.Repositories; 
using Application.Reviews.Exceptions; 
using Domain.Reviews; 
using Domain.Users;   
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Reviews.Commands
{
    public record UpdateReviewCommand : IRequest<Result<Review, ReviewException>>
    {
        public required Guid ReviewId { get; init; }

        public required int Rating { get; init; }
        public required string Comment { get; init; }

        public required Guid RequestingUserId { get; init; }
    }

    public class UpdateReviewCommandHandler(
        IReviewsRepository reviewsRepository)
        : IRequestHandler<UpdateReviewCommand, Result<Review, ReviewException>>
    {
        public async Task<Result<Review, ReviewException>> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
        {
            var reviewIdToUpdate = new ReviewId(request.ReviewId);
            var requestingUserId = new UserId(request.RequestingUserId);

            var existingReviewOption = await reviewsRepository.GetById(reviewIdToUpdate, cancellationToken);

            return await existingReviewOption.Match<Task<Result<Review, ReviewException>>>(
                some: async review => 
                {
                    if (review.UserId != requestingUserId)
                    {
                        return new UserNotAuthorizedForReviewOperationException(requestingUserId, reviewIdToUpdate, "UpdateReview");
                    }

                    return await UpdateReviewEntity(review, request, cancellationToken);
                },
                none: () => 
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
                review.UpdateReview(request.Rating, request.Comment);

                var updatedReview = await reviewsRepository.Update(review, cancellationToken);
                return updatedReview; 
            }
            catch (Exception exception)
            {
                return new ReviewOperationFailedException(review.Id, "UpdateReview", exception);
            }
        }
    }
}