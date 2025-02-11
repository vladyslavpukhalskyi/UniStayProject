using Domain.Reviews;
using Optional;
using Domain.Listings;
using Domain.Users;

namespace Application.Common.Interfaces.Repositories
{
    public interface IReviewsRepository
    {
        Task<Review> Add(Review review, CancellationToken cancellationToken);
        Task<Review> Update(Review review, CancellationToken cancellationToken);
        Task<Review> Delete(Review review, CancellationToken cancellationToken);
        Task<Option<Review>> GetById(ReviewId id, CancellationToken cancellationToken);
        Task<IReadOnlyList<Review>> GetAllReviewsForListing(ListingId listingId, CancellationToken cancellationToken);
        Task<IReadOnlyList<Review>> GetAllReviewsByUser(UserId userId, CancellationToken cancellationToken);
    }
}