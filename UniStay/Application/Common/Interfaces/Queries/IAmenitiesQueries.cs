using Domain.Amenities;
using Optional;

namespace Application.Common.Interfaces.Queries
{
    public interface IAmenitiesQueries
    {
        Task<IReadOnlyList<Amenity>> GetAll(CancellationToken cancellationToken);

        Task<Option<Amenity>> GetById(AmenityId id, CancellationToken cancellationToken);

        Task<Option<Amenity>> GetByTitle(string title, CancellationToken cancellationToken);
    }
}