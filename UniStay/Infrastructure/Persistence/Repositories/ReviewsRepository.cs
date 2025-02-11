using Application.Common.Interfaces.Repositories;
using Domain.Listings;
using Domain.Reviews;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace Infrastructure.Persistence.Repositories
{
    public class ReviewsRepository : IReviewsRepository
    {
        private readonly ApplicationDbContext _context;

        public ReviewsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Review> Add(Review review, CancellationToken cancellationToken)
        {
            await _context.Reviews.AddAsync(review, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return review;
        }

        public async Task<Review> Update(Review review, CancellationToken cancellationToken)
        {
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync(cancellationToken);
            return review;
        }

        public async Task<Review> Delete(Review review, CancellationToken cancellationToken)
        {
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync(cancellationToken);
            return review;
        }

        public async Task<Option<Review>> GetById(ReviewId id, CancellationToken cancellationToken)
        {
            var review = await _context.Reviews.FindAsync(new object[] { id }, cancellationToken);
            return review == null ? Option.None<Review>() : Option.Some(review);
        }

        public async Task<IReadOnlyList<Review>> GetAllReviewsForListing(ListingId listingId, CancellationToken cancellationToken)
        {
            return await _context.Reviews
                .Where(r => r.ListingId == listingId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Review>> GetAllReviewsByUser(UserId userId, CancellationToken cancellationToken)
        {
            return await _context.Reviews
                .Where(r => r.UserId == userId)
                .ToListAsync(cancellationToken);
        }
    }
}