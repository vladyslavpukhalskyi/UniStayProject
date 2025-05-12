// Файл: Infrastructure/Persistence/Repositories/AmenitiesRepository.cs
using Application.Common.Interfaces.Repositories; // Тільки репозиторій
using Domain.Amenities;
using Domain.Listings; // Може бути потрібен для Include, якщо треба
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Infrastructure.Persistence.Repositories
{
    // Прибираємо IAmenitiesQueries, якщо репозиторій тільки для команд
    public class AmenitiesRepository : IAmenitiesRepository
    {
        private readonly ApplicationDbContext _context;

        public AmenitiesRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Amenity> Add(Amenity amenity, CancellationToken cancellationToken)
        {
            if (amenity == null) throw new ArgumentNullException(nameof(amenity));
            await _context.Amenities.AddAsync(amenity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return amenity;
        }

        public async Task<Amenity> Update(Amenity amenity, CancellationToken cancellationToken)
        {
            if (amenity == null) throw new ArgumentNullException(nameof(amenity));
            _context.Amenities.Update(amenity);
            await _context.SaveChangesAsync(cancellationToken);
            return amenity;
        }

        public async Task<Amenity> Delete(Amenity amenity, CancellationToken cancellationToken)
        {
            if (amenity == null) throw new ArgumentNullException(nameof(amenity));
            _context.Amenities.Remove(amenity);
            await _context.SaveChangesAsync(cancellationToken);
            return amenity;
        }

        public async Task<Option<Amenity>> GetById(AmenityId id, CancellationToken cancellationToken)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            // Прибираємо Include(a => a.Listings), якщо він не потрібен для команд Update/Delete
            var entity = await _context.Amenities
                // .Include(a => a.Listings) // Розкоментуйте, якщо потрібно для логіки команд
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            return entity.SomeNotNull();
        }

        // Оновлена реалізація GetByTitleAsync
        public async Task<Option<Amenity>> GetByTitleAsync(string title, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return Option.None<Amenity>(); // Повертаємо None замість винятку
            }
            // Порівняння без урахування регістру, без Include, без AsNoTracking
            var amenity = await _context.Amenities
                .FirstOrDefaultAsync(a => a.Title.ToLower() == title.ToLower(), cancellationToken);
            return amenity.SomeNotNull();
        }

        // Нова реалізація GetByIdsAsync
        public async Task<IReadOnlyList<Amenity>> GetByIdsAsync(IEnumerable<AmenityId> ids, CancellationToken cancellationToken)
        {
            if (ids == null || !ids.Any())
            {
                return new List<Amenity>().AsReadOnly(); // Повертаємо порожній список
            }
            // Отримуємо список значень Guid з AmenityId
            var guidIds = ids.Select(id => id.Value).Distinct().ToList();

            // Шукаємо зручності, ID яких є у списку guidIds
            return await _context.Amenities
                .Where(a => guidIds.Contains(a.Id.Value)) // Використовуємо Contains для пошуку за списком
                .ToListAsync(cancellationToken);
                // Тут не додаємо AsNoTracking, оскільки ці сутності можуть бути використані для оновлення Listing
        }

        // Метод GetAll видалено, оскільки прибрали IAmenitiesQueries
        // public async Task<IReadOnlyList<Amenity>> GetAll(...) { ... }
    }
}