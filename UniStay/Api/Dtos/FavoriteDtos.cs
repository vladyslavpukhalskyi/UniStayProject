// Рекомендується розмістити кожен record в окремому файлі
// Наприклад: Api/Dtos/FavoriteDto.cs, Api/Dtos/ListingSummaryDto.cs і т.д.

// Потрібні using для доменних сутностей та їх ID
using Domain.Favorites;
using Domain.Listings; // Для ListingId, Listing
using Domain.Users;   // Для User та UserSummaryDto
using System;
using System.Collections.Generic;
using System.Linq;

// Переконайтесь, що UserSummaryDto доступний у цьому контексті
// (визначений в іншому файлі DTO у namespace Api.Dtos).

namespace Api.Dtos
{
    /// <summary>
    /// Допоміжне DTO для базової інформації про оголошення (Listing).
    /// </summary>
    public record ListingSummaryDto(
        Guid Id,
        string Title)
    {
        /// <summary>
        /// Створює ListingSummaryDto з доменної моделі Listing.
        /// </summary>
        public static ListingSummaryDto FromDomainModel(Listing listing) =>
            new(
                Id: listing.Id.Value, // Припускаємо, що ListingId має поле Value
                Title: listing.Title
            );
    }

    /// <summary>
    /// DTO для відображення інформації про стан "обраного" для оголошення.
    /// Показує, яке оголошення додано в обране та якими користувачами.
    /// </summary>
    public record FavoriteDto(
        Guid Id, // ID самого запису Favorite
        Guid ListingId, // ID оголошення, яке додано в обране
        ListingSummaryDto? Listing, // Коротка інформація про оголошення (якщо Listing завантажено)
        List<UserSummaryDto> Users // Список користувачів, які додали це оголошення в обране
    )
    {
        /// <summary>
        /// Статичний метод для створення FavoriteDto з доменної моделі Favorite.
        /// Передбачає, що навігаційні властивості 'Listing' та 'Users' можуть бути завантажені.
        /// </summary>
        public static FavoriteDto FromDomainModel(Favorite favorite) =>
            new(
                Id: favorite.Id.Value, // Припускаємо, що FavoriteId має поле Value
                ListingId: favorite.ListingId.Value,
                Listing: favorite.Listing == null ? null : ListingSummaryDto.FromDomainModel(favorite.Listing),
                Users: favorite.Users?.Select(UserSummaryDto.FromDomainModel).ToList() ?? new List<UserSummaryDto>()
            );
    }

    /// <summary>
    /// DTO, що використовується, коли користувач додає оголошення в обране.
    /// UserId користувача, що виконує дію, має визначатися сервером (з контексту аутентифікації).
    /// </summary>
    public record CreateFavoriteDto(
        Guid ListingId // ID оголошення, яке додається в обране
    );

    // Примітка: Для видалення з обраного (DELETE запит) зазвичай не потрібен
    // окремий DTO для тіла запиту. ListingId передається в URL,
    // а UserId береться з контексту аутентифікації.

    // Порожній клас FavoriteDtos тепер не потрібен.
    // public class FavoriteDtos { }
}