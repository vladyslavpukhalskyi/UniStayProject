// Рекомендується розмістити кожен record в окремому файлі
// Наприклад: Api/Dtos/ReviewDto.cs, Api/Dtos/CreateReviewDto.cs, Api/Dtos/UpdateReviewDto.cs

// Потрібні using для доменних сутностей та їх ID
using Domain.Reviews;
using Domain.Users;
using Domain.Listings;
using System; // Для Guid та DateTime

namespace Api.Dtos
{
    /// <summary>
    /// Допоміжне DTO для короткої інформації про користувача (щоб не включати повний UserDto).
    /// </summary>
    public record UserSummaryDto(
        Guid Id,
        string FullName)
    {
        /// <summary>
        /// Створює UserSummaryDto з доменної моделі User.
        /// </summary>
        public static UserSummaryDto FromDomainModel(User user) =>
            new(
                Id: user.Id.Value,
                FullName: user.FullName // Використовуємо FullName з моделі User
            );
    }

    /// <summary>
    /// DTO для відображення інформації про відгук.
    /// Включає ID користувача та оголошення, а також коротку інформацію про користувача.
    /// </summary>
    public record ReviewDto(
        Guid Id,
        Guid ListingId, // ID оголошення, до якого відноситься відгук
        Guid UserId,    // ID користувача, який залишив відгук
        UserSummaryDto? User, // Коротка інформація про користувача (якщо User завантажено)
        int Rating,
        string Comment,
        DateTime PublicationDate)
    {
        /// <summary>
        /// Статичний метод для створення ReviewDto з доменної моделі Review.
        /// Передбачає, що навігаційна властивість 'User' може бути завантажена.
        /// </summary>
        public static ReviewDto FromDomainModel(Review review) =>
            new(
                Id: review.Id.Value,
                ListingId: review.ListingId.Value,
                UserId: review.UserId.Value,
                // Перевіряємо, чи User не null перед мапуванням (як у прикладі MovieDto)
                User: review.User == null ? null : UserSummaryDto.FromDomainModel(review.User),
                Rating: review.Rating,
                Comment: review.Comment,
                PublicationDate: review.PublicationDate
            );
    }

    /// <summary>
    /// DTO для створення нового відгуку.
    /// UserId зазвичай визначається на основі аутентифікованого користувача,
    /// а не передається напряму в тілі запиту.
    /// </summary>
    public record CreateReviewDto(
        Guid ListingId, // Оголошення, яке оцінюється
        int Rating,     // Оцінка (наприклад, від 1 до 5)
        string Comment  // Текст відгуку
    );

    /// <summary>
    /// DTO для оновлення існуючого відгуку.
    /// Дозволяє змінити оцінку та коментар.
    /// </summary>
    public record UpdateReviewDto(
        int Rating,
        string Comment
        // Id відгуку передається в URL, UserId та ListingId не змінюються.
    );

    // Порожній клас ReviewDtos тепер не потрібен.
    // public class ReviewDtos { }
}