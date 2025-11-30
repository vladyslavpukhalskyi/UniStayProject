using Domain.Listings;

namespace Application.Common.Interfaces.Queries
{
    public interface IOstrohLandmarksQueries
    {
        Task<IReadOnlyList<OstrohLandmark>> GetAll(CancellationToken cancellationToken);
    }
}
