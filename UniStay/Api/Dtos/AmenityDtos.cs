// Файл: Api/Dtos/AmenityDto.cs (приклад)

// Потрібні using для доменних сутностей та їх ID
using Domain.Amenities;
using System; // Для Guid

namespace Api.Dtos
{
    /// <summary>
    /// DTO для відображення інформації про зручність (Amenity).
    /// **ВАЖЛИВО:** Переконайтеся, що це визначення існує лише ОДИН РАЗ у проекті.
    /// </summary>
    public record AmenityDto(
        Guid Id,
        string Title)
    {
        /// <summary>
        /// Статичний метод для створення AmenityDto з доменної моделі Amenity.
        /// </summary>
        public static AmenityDto FromDomainModel(Amenity amenity) =>
            new(
                Id: amenity.Id.Value,
                Title: amenity.Title
            );
    }

    /// <summary>
    /// DTO для створення нової зручності.
    /// </summary>
    public record CreateAmenityDto(
        string Title
    );

    /// <summary>
    /// DTO для оновлення назви існуючої зручності.
    /// </summary>
    public record UpdateAmenityDto(
        string Title
    );
}