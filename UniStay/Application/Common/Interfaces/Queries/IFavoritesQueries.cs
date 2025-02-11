using Domain.Favorites;
using Optional;
using Domain.Listings;
using Domain.Users;

namespace Application.Common.Interfaces.Queries
{
    public interface IFavoritesQueries
    {
        Task<IReadOnlyList<Favorite>> GetAll(CancellationToken cancellationToken);

        Task<Option<Favorite>> GetById(FavoriteId id, CancellationToken cancellationToken);

        Task<IReadOnlyList<Favorite>> GetByUserId(UserId userId, CancellationToken cancellationToken);

        Task<IReadOnlyList<Favorite>> GetByListingId(ListingId listingId, CancellationToken cancellationToken);
    }
}