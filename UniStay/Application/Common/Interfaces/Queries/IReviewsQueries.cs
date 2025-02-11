using Domain.Reviews;
using Domain.Listings;
using Domain.Users;
using Optional;

namespace Application.Common.Interfaces.Queries
{
    public interface IReviewsQueries
    {
        Task<IReadOnlyList<Review>> GetAllReviewsForListing(ListingId listingId, CancellationToken cancellationToken);

        Task<IReadOnlyList<Review>> GetAllReviewsByUser(UserId userId, CancellationToken cancellationToken);

        Task<Option<Review>> GetById(ReviewId id, CancellationToken cancellationToken);
    }
}