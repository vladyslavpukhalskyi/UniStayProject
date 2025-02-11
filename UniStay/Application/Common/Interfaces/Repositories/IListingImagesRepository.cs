using Domain.ListingImages;
using Optional;
using Domain.Listings;

namespace Application.Common.Interfaces.Repositories
{
    public interface IListingImagesRepository
    {
        Task<ListingImage> Add(ListingImage listingImage, CancellationToken cancellationToken);
        Task<ListingImage> Update(ListingImage listingImage, CancellationToken cancellationToken);
        Task<ListingImage> Delete(ListingImage listingImage, CancellationToken cancellationToken);
        Task<Option<ListingImage>> GetById(ListingImageId id, CancellationToken cancellationToken);
        Task<IReadOnlyList<ListingImage>> GetByListingId(ListingId listingId, CancellationToken cancellationToken);
    }
}