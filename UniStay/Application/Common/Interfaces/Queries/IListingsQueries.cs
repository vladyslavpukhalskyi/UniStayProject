using Domain.Listings;
using Optional;
using Domain.Users;

namespace Application.Common.Interfaces.Queries
{
    public interface IListingsQueries
    {
        Task<IReadOnlyList<Listing>> GetAll(CancellationToken cancellationToken);

        Task<Option<Listing>> GetById(ListingId id, CancellationToken cancellationToken);

        Task<IReadOnlyList<Listing>> GetByUserId(UserId userId, CancellationToken cancellationToken);

        Task<IReadOnlyList<Listing>> Search(string keyword, CancellationToken cancellationToken);
    }
}