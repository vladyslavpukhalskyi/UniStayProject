// Файл: Infrastructure/Persistence/Repositories/FavoritesRepository.cs

using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Favorites;
using Domain.Listings;
using Domain.Users;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    // Клас реалізує ОБИДВА інтерфейси
    public class FavoritesRepository : IFavoritesRepository, IFavoritesQueries
    {
        private readonly ApplicationDbContext _context;

        public FavoritesRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // --- Методи IFavoritesRepository ---

        public async Task<Favorite> Add(Favorite favorite, CancellationToken cancellationToken)
        {
            if (favorite == null) throw new ArgumentNullException(nameof(favorite));
            await _context.Favorites.AddAsync(favorite, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return favorite;
        }

        public async Task<Favorite> Update(Favorite favorite, CancellationToken cancellationToken)
        {
            if (favorite == null) throw new ArgumentNullException(nameof(favorite));
            _context.Favorites.Update(favorite);
            await _context.SaveChangesAsync(cancellationToken);
            return favorite;
        }

        public async Task<Favorite> Delete(Favorite favorite, CancellationToken cancellationToken)
        {
            if (favorite == null) throw new ArgumentNullException(nameof(favorite));
            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync(cancellationToken);
            return favorite;
        }

        public async Task<Option<Favorite>> GetById(FavoriteId id, CancellationToken cancellationToken)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var entity = await _context.Favorites
                .Include(f => f.Users)
                .Include(f => f.Listing)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            return entity.SomeNotNull();
        }

        // Метод для IFavoritesRepository (повертає Option<Favorite>)
        public async Task<Option<Favorite>> GetByListingIdAsync(ListingId listingId, CancellationToken cancellationToken)
        {
            if (listingId == null) throw new ArgumentNullException(nameof(listingId));
            var favorite = await _context.Favorites
                .Include(f => f.Users) // Include Users needed for command handler logic
                .FirstOrDefaultAsync(f => f.ListingId == listingId, cancellationToken);
            return favorite.SomeNotNull();
        }

        // --- Методи IFavoritesQueries ---

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
            if (userId == null) throw new ArgumentNullException(nameof(userId));
            return await _context.Favorites
                .Where(f => f.Users.Any(u => u.Id == userId))
                .AsNoTracking()
                .Include(f => f.Listing) // Include Listing info
                .ToListAsync(cancellationToken);
        }

        // Доданий назад метод для IFavoritesQueries (повертає IReadOnlyList<Favorite>)
        public async Task<IReadOnlyList<Favorite>> GetByListingId(ListingId listingId, CancellationToken cancellationToken)
        {
            if (listingId == null) throw new ArgumentNullException(nameof(listingId));
            return await _context.Favorites
                .Where(f => f.ListingId == listingId)
                .AsNoTracking()
                .Include(f => f.Users)
                .Include(f => f.Listing)
                .ToListAsync(cancellationToken);
        }
    }
}