// Файл: Application/Common/Interfaces/Repositories/IFavoritesRepository.cs

using Domain.Favorites;
using Domain.Listings;
using Domain.Users;
using Optional; // Потрібен для Option<>
using System.Collections.Generic; // Потрібен для IReadOnlyList
using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Interfaces.Repositories
{
    /// <summary>
    /// Репозиторій для роботи з сутністю Favorite.
    /// </summary>
    public interface IFavoritesRepository
    {
        /// <summary>
        /// Додає новий запис Favorite.
        /// </summary>
        Task<Favorite> Add(Favorite favorite, CancellationToken cancellationToken);

        /// <summary>
        /// Оновлює існуючий запис Favorite (наприклад, список Users).
        /// </summary>
        Task<Favorite> Update(Favorite favorite, CancellationToken cancellationToken);

        /// <summary>
        /// Видаляє запис Favorite.
        /// </summary>
        /// <remarks>
        /// Зауважте: логіка команди може видаляти користувача зі списку Users,
        /// а потім видаляти сам запис Favorite, якщо список Users став порожнім.
        /// </remarks>
        Task<Favorite> Delete(Favorite favorite, CancellationToken cancellationToken); // Або Task DeleteAsync(...)

        /// <summary>
        /// Знаходить запис Favorite за його власним ID.
        /// </summary>
        Task<Option<Favorite>> GetById(FavoriteId id, CancellationToken cancellationToken);

        /// <summary>
        /// Знаходить усі записи Favorite, де вказаний користувач є у списку Users.
        /// </summary>
        Task<IReadOnlyList<Favorite>> GetByUserId(UserId userId, CancellationToken cancellationToken);

        // --- ЗМІНЕНО ---
        // Попередній метод: Task<IReadOnlyList<Favorite>> GetByListingId(...)

        /// <summary>
        /// Знаходить унікальний запис Favorite за ідентифікатором оголошення (ListingId).
        /// Повертає Option.None, якщо жоден користувач ще не додав це оголошення в обране.
        /// </summary>
        /// <param name="listingId">ID оголошення.</param>
        /// <param name="cancellationToken">Токен скасування.</param>
        /// <returns>Optional, що містить Favorite, якщо знайдено.</returns>
        Task<Option<Favorite>> GetByListingIdAsync(ListingId listingId, CancellationToken cancellationToken);
    }
}