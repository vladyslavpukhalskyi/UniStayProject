// Файл: Application/Common/Interfaces/Repositories/IAmenitiesRepository.cs
using Domain.Amenities;
using Optional;
using System.Collections.Generic; // Потрібен для IEnumerable та IReadOnlyList
using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Interfaces.Repositories
{
    /// <summary>
    /// Репозиторій для роботи з сутністю Amenity (для команд).
    /// </summary>
    public interface IAmenitiesRepository
    {
        /// <summary>
        /// Додає нову зручність.
        /// </summary>
        Task<Amenity> Add(Amenity amenity, CancellationToken cancellationToken);

        /// <summary>
        /// Оновлює існуючу зручність.
        /// </summary>
        Task<Amenity> Update(Amenity amenity, CancellationToken cancellationToken);

        /// <summary>
        /// Видаляє зручність.
        /// </summary>
        Task<Amenity> Delete(Amenity amenity, CancellationToken cancellationToken); // Або Task DeleteAsync(...)

        /// <summary>
        /// Знаходить зручність за ID.
        /// </summary>
        Task<Option<Amenity>> GetById(AmenityId id, CancellationToken cancellationToken);

        /// <summary>
        /// Знаходить зручність за її точною назвою (рекомендовано порівняння без урахування регістру).
        /// </summary>
        Task<Option<Amenity>> GetByTitleAsync(string title, CancellationToken cancellationToken); // Перейменовано

        /// <summary>
        /// Знаходить зручності за списком їх ID.
        /// </summary>
        /// <param name="ids">Список ID зручностей для пошуку.</param>
        /// <param name="cancellationToken">Токен скасування.</param>
        /// <returns>Список знайдених зручностей.</returns>
        Task<IReadOnlyList<Amenity>> GetByIdsAsync(IEnumerable<AmenityId> ids, CancellationToken cancellationToken); // Додано
    }
}