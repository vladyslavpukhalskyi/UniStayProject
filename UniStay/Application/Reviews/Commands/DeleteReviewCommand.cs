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
    public record DeleteReviewCommand : IRequest<Result<Review, ReviewException>>
    {
        public required Guid ReviewId { get; init; }

        public required Guid RequestingUserId { get; init; }
    }

    public class DeleteReviewCommandHandler(
        IReviewsRepository reviewsRepository)
        : IRequestHandler<DeleteReviewCommand, Result<Review, ReviewException>>
    {
        public async Task<Result<Review, ReviewException>> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
        {
            var reviewIdToDelete = new ReviewId(request.ReviewId);
            var requestingUserId = new UserId(request.RequestingUserId);

            var existingReviewOption = await reviewsRepository.GetById(reviewIdToDelete, cancellationToken);

            return await existingReviewOption.Match<Task<Result<Review, ReviewException>>>(
                some: async review => 
                {
                    if (review.UserId != requestingUserId)
                    {
                        return new UserNotAuthorizedForReviewOperationException(requestingUserId, reviewIdToDelete, "DeleteReview");
                    }

                    return await DeleteReviewEntity(review, cancellationToken);
                },
                none: () => 
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
                return deletedReview; 
            }
            catch (Exception exception)
            {
                return new ReviewOperationFailedException(review.Id, "DeleteReview", exception);
            }
        }
    }
}