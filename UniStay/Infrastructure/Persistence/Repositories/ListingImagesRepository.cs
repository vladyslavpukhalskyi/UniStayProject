// Файл: Infrastructure/Persistence/Repositories/ListingImagesRepository.cs
using Application.Common.Interfaces.Queries; // <--- ДОДАНО
using Application.Common.Interfaces.Repositories;
using Domain.ListingImages;
using Domain.Listings;
using Infrastructure.Persistence; // <--- ДОДАНО, якщо ApplicationDbContext тут
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
    public class ListingImagesRepository : IListingImagesRepository, IListingImagesQueries // <--- ДОДАНО IListingImagesQueries
    {
        private readonly ApplicationDbContext _context;

        public ListingImagesRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // --- Методи IListingImagesRepository ---
        public async Task<ListingImage> Add(ListingImage listingImage, CancellationToken cancellationToken)
        {
            await _context.ListingImages.AddAsync(listingImage, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return listingImage;
        }

        public async Task<ListingImage> Update(ListingImage listingImage, CancellationToken cancellationToken)
        {
            _context.ListingImages.Update(listingImage);
            await _context.SaveChangesAsync(cancellationToken);
            return listingImage;
        }

        public async Task<ListingImage> Delete(ListingImage listingImage, CancellationToken cancellationToken)
        {
            _context.ListingImages.Remove(listingImage);
            await _context.SaveChangesAsync(cancellationToken);
            return listingImage;
        }

        async Task<Option<ListingImage>> IListingImagesRepository.GetById(ListingImageId id, CancellationToken cancellationToken)
        {
            // Для команд може знадобитися Listing для перевірки авторизації
            var listingImage = await _context.ListingImages
                                    .Include(li => li.Listing) // Включаємо Listing
                                    .FirstOrDefaultAsync(li => li.Id == id, cancellationToken);
            return listingImage.SomeNotNull();
        }

        // --- Методи IListingImagesQueries ---
        public async Task<IReadOnlyList<ListingImage>> GetAll(CancellationToken cancellationToken)
        {
            return await _context.ListingImages.AsNoTracking().ToListAsync(cancellationToken);
        }

        async Task<Option<ListingImage>> IListingImagesQueries.GetById(ListingImageId id, CancellationToken cancellationToken)
        {
            var listingImage = await _context.ListingImages
                                    .AsNoTracking() 
                                    .FirstOrDefaultAsync(li => li.Id == id, cancellationToken);
            return listingImage.SomeNotNull();
        }

        public async Task<IReadOnlyList<ListingImage>> GetByListingId(ListingId listingId, CancellationToken cancellationToken)
        {
            return await _context.ListingImages
                .AsNoTracking()
                .Where(li => li.ListingId == listingId)
                .ToListAsync(cancellationToken);
        }
    }
}