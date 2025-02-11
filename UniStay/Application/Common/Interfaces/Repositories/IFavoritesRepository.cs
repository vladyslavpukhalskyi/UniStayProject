using Domain.Favorites;
using Optional;
using Domain.Listings;
using Domain.Users;

namespace Application.Common.Interfaces.Repositories
{
    public interface IFavoritesRepository
    {
        Task<Favorite> Add(Favorite favorite, CancellationToken cancellationToken);
        Task<Favorite> Update(Favorite favorite, CancellationToken cancellationToken);
        Task<Favorite> Delete(Favorite favorite, CancellationToken cancellationToken);
        Task<Option<Favorite>> GetById(FavoriteId id, CancellationToken cancellationToken);
        Task<IReadOnlyList<Favorite>> GetByUserId(UserId userId, CancellationToken cancellationToken);
        Task<IReadOnlyList<Favorite>> GetByListingId(ListingId listingId, CancellationToken cancellationToken);
    }
}