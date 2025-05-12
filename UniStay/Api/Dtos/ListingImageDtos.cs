// Файл: Api/Dtos/ListingImageDto.cs (приклад)

// Потрібні using для доменних сутностей та їх ID
using Domain.ListingImages;
using Domain.Listings; // Для ListingId
using System;          // Для Guid

namespace Api.Dtos
{
    /// <summary>
    /// DTO для відображення інформації про зображення оголошення.
    /// </summary>
    public record ListingImageDto(
        Guid Id,
        Guid ListingId, // ID оголошення, до якого належить зображення
        string ImageUrl)
    {
        /// <summary>
        /// Статичний метод для створення ListingImageDto з доменної моделі ListingImage.
        /// </summary>
        public static ListingImageDto FromDomainModel(ListingImage image) =>
            new(
                Id: image.Id.Value,
                ListingId: image.ListingId.Value,
                ImageUrl: image.ImageUrl
            );
    }

    /// <summary>
    /// DTO для створення нового зображення оголошення.
    /// </summary>
    public record CreateListingImageDto(
        Guid ListingId,
        string ImageUrl
    );

    /// <summary>
    /// DTO для оновлення URL існуючого зображення оголошення.
    /// </summary>
    public record UpdateListingImageDto(
        string ImageUrl
    );
}