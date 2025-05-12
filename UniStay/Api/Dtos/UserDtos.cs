// Рекомендується розмістити кожен record в окремому файлі
// Наприклад: Api/Dtos/UserDto.cs, Api/Dtos/CreateUserDto.cs, Api/Dtos/UpdateUserDto.cs

// Додайте цей using, якщо файл знаходиться поза межами Domain проекту
using Domain.Users;
using System; // Для Guid та DateTime

namespace Api.Dtos
{
    /// <summary>
    /// DTO для відображення інформації про користувача.
    /// Не містить пароль та інші чутливі дані, які не потрібні для відображення.
    /// </summary>
    public record UserDto(
        Guid Id, // Використовуємо Guid, оскільки UserId(Guid Value)
        string FirstName,
        string LastName,
        string Email,
        string FullName, // Додамо FullName з доменної моделі
        UserEnums.UserRole Role,
        string? PhoneNumber, // Використовуємо string? якщо номер може бути відсутнім
        string? ProfileImage, // Використовуємо string? якщо зображення може бути відсутнім
        DateTime RegistrationDate)
    {
        /// <summary>
        /// Статичний метод для створення UserDto з доменної моделі User.
        /// </summary>
        public static UserDto FromDomainModel(User user) =>
            new(
                Id: user.Id.Value,
                FirstName: user.FirstName,
                LastName: user.LastName,
                Email: user.Email,
                FullName: user.FullName, // Мапимо FullName
                Role: user.Role,
                PhoneNumber: user.PhoneNumber,
                ProfileImage: user.ProfileImage,
                RegistrationDate: user.RegistrationDate
            );
    }

    /// <summary>
    /// DTO для створення нового користувача.
    /// Містить поля, необхідні для реєстрації.
    /// </summary>
    public record CreateUserDto(
        string FirstName,
        string LastName,
        string Email,
        string Password, // Пароль потрібен для створення
        string? PhoneNumber,
        string? ProfileImage
        // Роль (Role) зазвичай не встановлюється користувачем при реєстрації,
        // а призначається за замовчуванням або адміністратором.
    );

    /// <summary>
    /// DTO для оновлення даних користувача.
    /// Містить поля, які дозволено змінювати.
    /// Пароль зазвичай оновлюється окремим процесом/ендпоінтом.
    /// Email часто не дозволяють змінювати.
    /// </summary>
    public record UpdateUserDto(
        string FirstName,
        string LastName,
        string? PhoneNumber,
        string? ProfileImage
        // Id користувача зазвичай передається в маршруті (URL), а не в тілі запиту.
    );

    // Порожній клас UserDtos тепер не потрібен, якщо ви визначаєте рекорди безпосередньо.
    // public class UserDtos { }
}