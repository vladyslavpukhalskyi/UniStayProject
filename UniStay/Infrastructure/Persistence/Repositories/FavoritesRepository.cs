using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Favorites;
using Domain.Listings;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace Infrastructure.Persistence.Repositories
{
    public class FavoritesRepository : IFavoritesRepository, IFavoritesQueries
    {
        private readonly ApplicationDbContext _context;

        public FavoritesRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Option<Favorite>> GetById(FavoriteId id, CancellationToken cancellationToken)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var entity = await _context.Favorites
                .Include(f => f.Users)
                .Include(f => f.Listing)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            return entity == null ? Option.None<Favorite>() : Option.Some(entity);
        }

        public async Task<IReadOnlyList<Favorite>> GetAll(CancellationToken cancellationToken)
        {
            return await _context.Favorites
                .AsNoTracking()
                .Include(f => f.Users)
                .Include(f => f.Listing)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Favorite>> GetByUserId(UserId userId, CancellationToken cancellationToken)
        {
            if (userId == null)
                throw new ArgumentNullException(nameof(userId));

            return await _context.Favorites
                .Where(f => f.Users.Any(u => u.Id == userId))
                .AsNoTracking()
                .Include(f => f.Users)
                .Include(f => f.Listing)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Favorite>> GetByListingId(ListingId listingId, CancellationToken cancellationToken)
        {
            if (listingId == null)
                throw new ArgumentNullException(nameof(listingId));

            return await _context.Favorites
                .Where(f => f.ListingId == listingId)
                .AsNoTracking()
                .Include(f => f.Users)
                .Include(f => f.Listing)
                .ToListAsync(cancellationToken);
        }

        public async Task<Favorite> Add(Favorite favorite, CancellationToken cancellationToken)
        {
            if (favorite == null)
                throw new ArgumentNullException(nameof(favorite));

            await _context.Favorites.AddAsync(favorite, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return favorite;
        }

        public async Task<Favorite> Update(Favorite favorite, CancellationToken cancellationToken)
        {
            if (favorite == null)
                throw new ArgumentNullException(nameof(favorite));

            _context.Favorites.Update(favorite);
            await _context.SaveChangesAsync(cancellationToken);

            return favorite;
        }

        public async Task<Favorite> Delete(Favorite favorite, CancellationToken cancellationToken)
        {
            if (favorite == null)
                throw new ArgumentNullException(nameof(favorite));

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync(cancellationToken);

            return favorite;
        }
    }
}