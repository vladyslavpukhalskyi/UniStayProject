// Файл: Infrastructure/Persistence/Repositories/ReviewsRepository.cs
using Application.Common.Interfaces.Queries; // <--- ДОДАНО
using Application.Common.Interfaces.Repositories;
using Domain.Reviews;
using Domain.Listings;
using Domain.Users;
using Infrastructure.Persistence; // <--- ДОДАНО
using Microsoft.EntityFrameworkCore;
using Optional;
using Optional.Async.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class ReviewsRepository : IReviewsRepository, IReviewsQueries // <--- ДОДАНО IReviewsQueries
    {
        private readonly ApplicationDbContext _context;

        public ReviewsRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // --- Методи IReviewsRepository ---
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
        
        // GetById для IReviewsRepository
        async Task<Option<Review>> IReviewsRepository.GetById(ReviewId id, CancellationToken cancellationToken)
        {
            // Для команд може бути достатньо без Include, якщо User не потрібен для валідації операції
            var review = await _context.Reviews
                                 // .Include(r => r.User) // Розкоментуйте, якщо User потрібен для логіки команд
                                 .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
            return review.SomeNotNull();
        }

        // --- Методи IReviewsQueries ---
        // GetById для IReviewsQueries
        async Task<Option<Review>> IReviewsQueries.GetById(ReviewId id, CancellationToken cancellationToken)
        {
            var review = await _context.Reviews
                .AsNoTracking()
                .Include(r => r.User) // Для ReviewDto
                // .Include(r => r.Listing) // Якщо ReviewDto також потребує інформацію про Listing
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
            return review.SomeNotNull();
        }

        public async Task<IReadOnlyList<Review>> GetAllReviewsForListing(ListingId listingId, CancellationToken cancellationToken)
        {
            return await _context.Reviews
                .AsNoTracking()
                .Where(r => r.ListingId == listingId)
                .Include(r => r.User) // Для ReviewDto
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Review>> GetAllReviewsByUser(UserId userId, CancellationToken cancellationToken)
        {
            return await _context.Reviews
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .Include(r => r.User) // Для ReviewDto
                // .Include(r => r.Listing) // Якщо ReviewDto також потребує інформацію про Listing
                .ToListAsync(cancellationToken);
        }
    }
}