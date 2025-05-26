using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Favorites;
using Domain.Listings;
using Domain.Users;
using Infrastructure.Persistence;
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
    public class FavoritesRepository : IFavoritesRepository, IFavoritesQueries
    {
        private readonly ApplicationDbContext _context;

        public FavoritesRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Favorite> Add(Favorite favorite, CancellationToken cancellationToken)
        {
            await _context.Favorites.AddAsync(favorite, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return favorite;
        }

        public async Task<Favorite> Update(Favorite favorite, CancellationToken cancellationToken)
        {
            _context.Favorites.Update(favorite);
            await _context.SaveChangesAsync(cancellationToken);
            return favorite;
        }

        public async Task<Favorite> Delete(Favorite favorite, CancellationToken cancellationToken)
        {
            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync(cancellationToken);
            return favorite;
        }

        async Task<Option<Favorite>> IFavoritesRepository.GetById(FavoriteId id, CancellationToken cancellationToken)
        {
            return await _context.Favorites
                .Include(f => f.Users)
                .Include(f => f.Listing)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                .SomeNotNullAsync();
        }

        public async Task<Option<Favorite>> GetByListingIdAsync(ListingId listingId, CancellationToken cancellationToken)
        {
            return await _context.Favorites
                .Include(f => f.Users)
                .FirstOrDefaultAsync(f => f.ListingId == listingId, cancellationToken)
                .SomeNotNullAsync();
        }

        public async Task<IReadOnlyList<Favorite>> GetAll(CancellationToken cancellationToken)
        {
            return await _context.Favorites
                .AsNoTracking()
                .Include(f => f.Users)
                .Include(f => f.Listing)
                .ToListAsync(cancellationToken);
        }
        
        async Task<Option<Favorite>> IFavoritesQueries.GetById(FavoriteId id, CancellationToken cancellationToken)
        {
            return await _context.Favorites
                .AsNoTracking()
                .Include(f => f.Users)
                .Include(f => f.Listing)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                .SomeNotNullAsync();
        }

        public async Task<IReadOnlyList<Favorite>> GetByUserId(UserId userId, CancellationToken cancellationToken)
        {
            return await _context.Favorites
                .Where(f => f.Users.Any(u => u.Id == userId))
                .AsNoTracking()
                .Include(f => f.Listing)
                .Include(f => f.Users)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Favorite>> GetByListingId(ListingId listingId, CancellationToken cancellationToken)
        {
            return await _context.Favorites
                .Where(f => f.ListingId == listingId)
                .AsNoTracking()
                .Include(f => f.Users)
                .Include(f => f.Listing)
                .ToListAsync(cancellationToken);
        }
    }
}