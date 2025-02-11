using Application.Common.Interfaces.Repositories;
using Domain.Listings;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace Infrastructure.Persistence.Repositories
{
    public class ListingsRepository : IListingsRepository
    {
        private readonly ApplicationDbContext _context;

        public ListingsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Listing> Add(Listing listing, CancellationToken cancellationToken)
        {
            await _context.Listings.AddAsync(listing, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return listing;
        }

        public async Task<Listing> Update(Listing listing, CancellationToken cancellationToken)
        {
            _context.Listings.Update(listing);
            await _context.SaveChangesAsync(cancellationToken);
            return listing;
        }

        public async Task<Listing> Delete(Listing listing, CancellationToken cancellationToken)
        {
            _context.Listings.Remove(listing);
            await _context.SaveChangesAsync(cancellationToken);
            return listing;
        }

        public async Task<Option<Listing>> GetById(ListingId id, CancellationToken cancellationToken)
        {
            var listing = await _context.Listings
                .Include(l => l.User)
                .Include(l => l.ListingImages)
                .Include(l => l.Amenities)
                .Include(l => l.Reviews)
                .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
            return listing != null ? Option.Some(listing) : Option.None<Listing>();
        }

        public async Task<IReadOnlyList<Listing>> GetByUserId(UserId userId, CancellationToken cancellationToken)
        {
            return await _context.Listings
                .Where(l => l.UserId == userId)
                .Include(l => l.ListingImages)
                .Include(l => l.Amenities)
                .Include(l => l.Reviews)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Listing>> Search(string keyword, CancellationToken cancellationToken)
        {
            return await _context.Listings
                .Where(l => l.Title.Contains(keyword) || l.Description.Contains(keyword) || l.Address.Contains(keyword))
                .Include(l => l.ListingImages)
                .Include(l => l.Amenities)
                .Include(l => l.Reviews)
                .ToListAsync(cancellationToken);
        }
    }
}