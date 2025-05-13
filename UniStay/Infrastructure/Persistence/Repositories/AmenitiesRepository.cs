// Файл: Infrastructure/Persistence/Repositories/AmenitiesRepository.cs
using Application.Common.Interfaces.Queries; // <--- ДОДАНО
using Application.Common.Interfaces.Repositories;
using Domain.Amenities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Optional;
using Optional.Async.Extensions; // Для .SomeNotNullAsync()
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class AmenitiesRepository : IAmenitiesRepository, IAmenitiesQueries // <--- ДОДАНО IAmenitiesQueries
    {
        private readonly ApplicationDbContext _context;

        public AmenitiesRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // --- Методи IAmenitiesRepository ---
        public async Task<Amenity> Add(Amenity amenity, CancellationToken cancellationToken)
        {
            if (amenity == null) throw new ArgumentNullException(nameof(amenity));
            await _context.Amenities.AddAsync(amenity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken); // Зберігаємо зміни тут
            return amenity;
        }

        public async Task<Amenity> Update(Amenity amenity, CancellationToken cancellationToken)
        {
            if (amenity == null) throw new ArgumentNullException(nameof(amenity));
            _context.Amenities.Update(amenity);
            await _context.SaveChangesAsync(cancellationToken); // Зберігаємо зміни тут
            return amenity;
        }

        public async Task<Amenity> Delete(Amenity amenity, CancellationToken cancellationToken)
        {
            if (amenity == null) throw new ArgumentNullException(nameof(amenity));
            _context.Amenities.Remove(amenity);
            await _context.SaveChangesAsync(cancellationToken); // Зберігаємо зміни тут
            return amenity;
        }

        // GetById для IAmenitiesRepository (може використовуватися командами)
        async Task<Option<Amenity>> IAmenitiesRepository.GetById(AmenityId id, CancellationToken cancellationToken)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var entity = await _context.Amenities
                // Немає .Include(a => a.Listings), оскільки Amenity не має прямого зв'язку для команд
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            return entity.SomeNotNull();
        }
        
        public async Task<Option<Amenity>> GetByTitleAsync(string title, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return Option.None<Amenity>();
            }
            var amenity = await _context.Amenities
                .FirstOrDefaultAsync(a => EF.Functions.ILike(a.Title, title), cancellationToken); // ILike для пошуку без регістру
            return amenity.SomeNotNull();
        }
        
        public async Task<IReadOnlyList<Amenity>> GetByIdsAsync(IEnumerable<AmenityId> ids, CancellationToken cancellationToken)
        {
            if (ids == null || !ids.Any())
            {
                return new List<Amenity>().AsReadOnly();
            }
            var guidIds = ids.Select(id => id.Value).Distinct().ToList();
            return await _context.Amenities
                .Where(a => guidIds.Contains(a.Id.Value))
                .ToListAsync(cancellationToken);
        }

        // --- Методи IAmenitiesQueries ---
        public async Task<IReadOnlyList<Amenity>> GetAll(CancellationToken cancellationToken)
        {
            return await _context.Amenities.AsNoTracking().ToListAsync(cancellationToken);
        }

        // GetById для IAmenitiesQueries (тільки для читання)
        async Task<Option<Amenity>> IAmenitiesQueries.GetById(AmenityId id, CancellationToken cancellationToken)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var entity = await _context.Amenities
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            return entity.SomeNotNull();
        }
        
        // GetByTitle для IAmenitiesQueries (тільки для читання)
        async Task<Option<Amenity>> IAmenitiesQueries.GetByTitle(string title, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return Option.None<Amenity>();
            }
            var amenity = await _context.Amenities
                .AsNoTracking()
                .FirstOrDefaultAsync(a => EF.Functions.ILike(a.Title, title), cancellationToken); // ILike для пошуку без регістру
            return amenity.SomeNotNull();
        }
    }
}