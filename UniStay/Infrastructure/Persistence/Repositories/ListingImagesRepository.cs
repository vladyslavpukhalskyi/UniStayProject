using Application.Common.Interfaces.Repositories;
using Domain.ListingImages;
using Domain.Listings;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace Infrastructure.Persistence.Repositories
{
    public class ListingImagesRepository : IListingImagesRepository
    {
        private readonly ApplicationDbContext _context;

        public ListingImagesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

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

        public async Task<Option<ListingImage>> GetById(ListingImageId id, CancellationToken cancellationToken)
        {
            var listingImage = await _context.ListingImages.FirstOrDefaultAsync(li => li.Id == id, cancellationToken);
            return listingImage.SomeNotNull();
        }

        public async Task<IReadOnlyList<ListingImage>> GetByListingId(ListingId listingId, CancellationToken cancellationToken)
        {
            return await _context.ListingImages
                .Where(li => li.ListingId == listingId)
                .ToListAsync(cancellationToken);
        }
    }
}
