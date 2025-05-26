using Domain.Favorites;
using Domain.Listings;
using Domain.Users;
using Optional; 
using System.Collections.Generic; 
using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Interfaces.Repositories
{
    public interface IFavoritesRepository
    {
        Task<Favorite> Add(Favorite favorite, CancellationToken cancellationToken);

        Task<Favorite> Update(Favorite favorite, CancellationToken cancellationToken);

        Task<Favorite> Delete(Favorite favorite, CancellationToken cancellationToken); 

        Task<Option<Favorite>> GetById(FavoriteId id, CancellationToken cancellationToken);

        Task<IReadOnlyList<Favorite>> GetByUserId(UserId userId, CancellationToken cancellationToken);

        Task<Option<Favorite>> GetByListingIdAsync(ListingId listingId, CancellationToken cancellationToken);
    }
}