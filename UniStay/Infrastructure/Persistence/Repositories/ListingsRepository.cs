// Файл: Infrastructure/Persistence/Repositories/ListingsRepository.cs
using Application.Common.Interfaces.Queries; // <--- ДОДАНО
using Application.Common.Interfaces.Repositories;
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
    public class ListingsRepository : IListingsRepository, IListingsQueries // <--- ДОДАНО IListingsQueries
    {
        private readonly ApplicationDbContext _context;

        public ListingsRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // --- Методи IListingsRepository ---
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

        // GetById для IListingsRepository (для команд)
        async Task<Option<Listing>> IListingsRepository.GetById(ListingId id, CancellationToken cancellationToken)
        {
            // Команди можуть потребувати відстеження та повні дані для оновлення зв'язків (напр. Amenities)
            var listing = await _context.Listings
                .Include(l => l.User)
                .Include(l => l.Amenities) 
                .Include(l => l.ListingImages)
                .Include(l => l.Reviews)
                .Include(l => l.Favorites)
                .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
            return listing.SomeNotNull();
        }

        // --- Методи IListingsQueries ---
        public async Task<IReadOnlyList<Listing>> GetAll(CancellationToken cancellationToken)
        {
            return await _context.Listings
                .AsNoTracking()
                .Include(l => l.User)
                .Include(l => l.Amenities)
                .Include(l => l.ListingImages)
                .Include(l => l.Reviews)
                .Include(l => l.Favorites) // Для підрахунку FavoriteCount в DTO
                .ToListAsync(cancellationToken);
        }

        // GetById для IListingsQueries (тільки для читання)
        async Task<Option<Listing>> IListingsQueries.GetById(ListingId id, CancellationToken cancellationToken)
        {
            var listing = await _context.Listings
                .AsNoTracking()
                .Include(l => l.User)
                .Include(l => l.Amenities)
                .Include(l => l.ListingImages)
                .Include(l => l.Reviews)
                .Include(l => l.Favorites) // Для підрахунку FavoriteCount в DTO
                .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
            return listing.SomeNotNull();
        }

        public async Task<IReadOnlyList<Listing>> GetByUserId(UserId userId, CancellationToken cancellationToken)
        {
            return await _context.Listings
                .AsNoTracking()
                .Where(l => l.UserId == userId)
                .Include(l => l.User)
                .Include(l => l.Amenities)
                .Include(l => l.ListingImages)
                .Include(l => l.Reviews)
                .Include(l => l.Favorites)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Listing>> Search(string keyword, CancellationToken cancellationToken)
        {
            // Перевірка на null або порожній рядок для keyword
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return await GetAll(cancellationToken); // Або повернути порожній список
            }
            var lowerKeyword = keyword.ToLower();
            return await _context.Listings
                .AsNoTracking()
                .Where(l => l.Title.ToLower().Contains(lowerKeyword) || 
                            l.Description.ToLower().Contains(lowerKeyword) || 
                            l.Address.ToLower().Contains(lowerKeyword))
                .Include(l => l.User)
                .Include(l => l.Amenities)
                .Include(l => l.ListingImages)
                .Include(l => l.Reviews)
                .Include(l => l.Favorites)
                .ToListAsync(cancellationToken);
        }
    }
}