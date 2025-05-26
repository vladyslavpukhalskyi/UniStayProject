using Application.Common; 
using Application.Common.Interfaces.Repositories; 
using Application.Reviews.Exceptions; 
using Domain.Listings; 
using Domain.Reviews; 
using Domain.Users;   
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Reviews.Commands
{
    public record CreateReviewCommand : IRequest<Result<Review, ReviewException>>
    {
        public required Guid ListingId { get; init; }
        public required int Rating { get; init; }
        public required string Comment { get; init; }
        
        public required Guid UserId { get; init; }
    }

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

            var listingOption = await listingsRepository.GetById(listingId, cancellationToken);

            return await listingOption.Match<Task<Result<Review, ReviewException>>>(
                some: async listing => 
                {
                    if (listing.UserId == userId)
                    {
                        return new UserCannotReviewException(userId, listingId, "User cannot review their own listing.");
                    }
                    
                    var reviewId = ReviewId.New();
                    try
                    {
                        var review = Review.New(
                            id: reviewId,
                            userId: userId,
                            listingId: listingId, 
                            rating: request.Rating,
                            comment: request.Comment
                        );

                        var addedReview = await reviewsRepository.Add(review, cancellationToken);
                        return addedReview; 
                    }
                    catch (Exception exception)
                    {
                        return new ReviewOperationFailedException(reviewId, "CreateReview", exception);
                    }
                },
                none: () => 
                {
                    ReviewException exception = new ListingNotFoundForReviewException(listingId);
                    return Task.FromResult<Result<Review, ReviewException>>(exception);
                }
            );
        }
    }
}