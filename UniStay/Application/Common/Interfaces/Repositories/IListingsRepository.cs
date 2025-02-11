using Domain.Listings;
using Optional;
using Domain.Users;

namespace Application.Common.Interfaces.Repositories
{
    public interface IListingsRepository
    {
        Task<Listing> Add(Listing listing, CancellationToken cancellationToken);
        Task<Listing> Update(Listing listing, CancellationToken cancellationToken);
        Task<Listing> Delete(Listing listing, CancellationToken cancellationToken);
        Task<Option<Listing>> GetById(ListingId id, CancellationToken cancellationToken);
        Task<IReadOnlyList<Listing>> GetByUserId(UserId userId, CancellationToken cancellationToken);
        Task<IReadOnlyList<Listing>> Search(string keyword, CancellationToken cancellationToken);
    }
}