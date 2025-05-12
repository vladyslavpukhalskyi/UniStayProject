// Файл: Api/Dtos/ListingDtos.cs (приклад)

// Потрібні using для доменних сутностей та їх ID/Enum
using Domain.Listings;
using Domain.Users;         // Для UserId та UserSummaryDto
using Domain.Amenities;     // Потрібен для використання Amenity у FromDomainModel для ListingDto
using Domain.Reviews;       // Для ReviewDto
using Domain.ListingImages; // Для ListingImageDto
using Domain.Favorites;     // Для підрахунку Favorite
using System;
using System.Collections.Generic;
using System.Linq;

// Переконайтесь, що UserSummaryDto, ReviewDto, ListingImageDto та AmenityDto
// доступні в цьому контексті (визначені в інших файлах DTO у namespace Api.Dtos).

namespace Api.Dtos
{
    // ВИДАЛЕНО ВИЗНАЧЕННЯ AmenityDto ЗВІДСИ
    // /// <summary>
    // /// DTO для інформації про зручність (Amenity).
    // /// </summary>
    // public record AmenityDto( ... ) { ... } // ВИДАЛЕНО

    // Визначення ListingImageDto тут також не повинно бути (має бути в окремому файлі)

    /// <summary>
    /// DTO для відображення детальної інформації про оголошення.
    /// </summary>
    public record ListingDto(
        Guid Id,
        string Title,
        string Description,
        string Address,
        float Price,
        ListingEnums.ListingType Type,
        Guid UserId,
        UserSummaryDto? User,
        List<ListingEnums.CommunalService> CommunalServices,
        ListingEnums.OwnershipType Owners,
        ListingEnums.NeighbourType Neighbours,
        DateTime PublicationDate,
        List<AmenityDto> Amenities, // ТЕПЕР ВИКОРИСТОВУЄ ВИЗНАЧЕННЯ З ІНШОГО ФАЙЛУ
        List<ReviewDto> Reviews,
        List<ListingImageDto> ListingImages,
        int FavoriteCount
    )
    {
        /// <summary>
        /// Статичний метод для створення ListingDto з доменної моделі Listing.
        /// </summary>
        public static ListingDto FromDomainModel(Listing listing) =>
            new(
                Id: listing.Id.Value,
                Title: listing.Title,
                Description: listing.Description,
                Address: listing.Address,
                Price: listing.Price,
                Type: listing.Type,
                UserId: listing.UserId.Value,
                User: listing.User == null ? null : UserSummaryDto.FromDomainModel(listing.User),
                CommunalServices: listing.CommunalServices ?? new List<ListingEnums.CommunalService>(),
                Owners: listing.Owners,
                Neighbours: listing.Neighbours,
                PublicationDate: listing.PublicationDate,
                // Використовує AmenityDto.FromDomainModel з іншого файлу
                Amenities: listing.Amenities?.Select(AmenityDto.FromDomainModel).ToList() ?? new List<AmenityDto>(),
                // Використовує ReviewDto.FromDomainModel з іншого файлу
                Reviews: listing.Reviews?.Select(ReviewDto.FromDomainModel).ToList() ?? new List<ReviewDto>(),
                 // Використовує ListingImageDto.FromDomainModel з іншого файлу
                ListingImages: listing.ListingImages?.Select(ListingImageDto.FromDomainModel).ToList() ?? new List<ListingImageDto>(),
                FavoriteCount: listing.Favorites?.Count ?? 0
            );
    }

    /// <summary>
    /// DTO для створення нового оголошення.
    /// </summary>
    public record CreateListingDto(
        string Title,
        string Description,
        string Address,
        float Price,
        ListingEnums.ListingType Type,
        List<ListingEnums.CommunalService> CommunalServices,
        ListingEnums.OwnershipType Owners,
        ListingEnums.NeighbourType Neighbours,
        List<Guid> AmenityIds
    );

    /// <summary>
    /// DTO для оновлення існуючого оголошення.
    /// </summary>
    public record UpdateListingDto(
        string Title,
        string Description,
        string Address,
        float Price,
        ListingEnums.ListingType Type,
        List<ListingEnums.CommunalService> CommunalServices,
        ListingEnums.OwnershipType Owners,
        ListingEnums.NeighbourType Neighbours,
        List<Guid> AmenityIds
    );
}