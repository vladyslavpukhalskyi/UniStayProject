using Application.Common.Interfaces.Queries;
using Domain.Listings;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class OstrohLandmarksRepository : IOstrohLandmarksQueries
    {
        private readonly ApplicationDbContext _context;

        public OstrohLandmarksRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IReadOnlyList<OstrohLandmark>> GetAll(CancellationToken cancellationToken)
        {
            return await _context.OstrohLandmarks.AsNoTracking().ToListAsync(cancellationToken);
        }
    }
}
