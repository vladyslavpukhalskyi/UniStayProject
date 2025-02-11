using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Amenities;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace Infrastructure.Persistence.Repositories
{
    public class AmenitiesRepository : IAmenitiesRepository, IAmenitiesQueries
    {
        private readonly ApplicationDbContext _context;

        public AmenitiesRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Amenity> Add(Amenity amenity, CancellationToken cancellationToken)
        {
            if (amenity == null)
                throw new ArgumentNullException(nameof(amenity));

            await _context.Amenities.AddAsync(amenity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return amenity;
        }

        public async Task<Amenity> Update(Amenity amenity, CancellationToken cancellationToken)
        {
            if (amenity == null)
                throw new ArgumentNullException(nameof(amenity));

            _context.Amenities.Update(amenity);
            await _context.SaveChangesAsync(cancellationToken);
            return amenity;
        }

        public async Task<Amenity> Delete(Amenity amenity, CancellationToken cancellationToken)
        {
            if (amenity == null)
                throw new ArgumentNullException(nameof(amenity));

            _context.Amenities.Remove(amenity);
            await _context.SaveChangesAsync(cancellationToken);
            return amenity;
        }

        public async Task<Option<Amenity>> GetById(AmenityId id, CancellationToken cancellationToken)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var entity = await _context.Amenities
                .Include(a => a.Listings)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            return entity == null ? Option.None<Amenity>() : Option.Some(entity);
        }

        public async Task<Option<Amenity>> GetByTitle(string title, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be null or empty.", nameof(title));

            var entity = await _context.Amenities
                .AsNoTracking()
                .Include(a => a.Listings)
                .FirstOrDefaultAsync(a => a.Title == title, cancellationToken);

            return entity == null ? Option.None<Amenity>() : Option.Some(entity);
        }

        public async Task<IReadOnlyList<Amenity>> GetAll(CancellationToken cancellationToken)
        {
            return await _context.Amenities
                .AsNoTracking()
                .Include(a => a.Listings)
                .ToListAsync(cancellationToken);
        }
    }
}
