using Domain.Listings;

namespace Api.Dtos.Listing
{
    public record ListingDto(
        Guid Id,
        string Title,
        string Description,
        string Address,
        double Latitude,
        double Longitude,
        float Price,
        ListingEnums.ListingType Type,
        Guid UserId,
        UserSummaryDto? User,
        List<ListingEnums.CommunalService> CommunalServices,
        ListingEnums.OwnershipType Owners,
        ListingEnums.NeighbourType Neighbours,
        DateTime PublicationDate,
        List<AmenityDto> Amenities,
        List<ReviewDto> Reviews,
        List<ListingImageDto> ListingImages,
        int FavoriteCount
    )
    {
        public static ListingDto FromDomainModel(Domain.Listings.Listing listing) =>
            new(
                Id: listing.Id.Value,
                Title: listing.Title,
                Description: listing.Description,
                Address: listing.Address,
                Latitude: listing.Latitude,
                Longitude: listing.Longitude,
                Price: listing.Price,
                Type: listing.Type,
                UserId: listing.UserId.Value,
                User: listing.User == null ? null : UserSummaryDto.FromDomainModel(listing.User),
                CommunalServices: listing.CommunalServices ?? new List<ListingEnums.CommunalService>(),
                Owners: listing.Owners,
                Neighbours: listing.Neighbours,
                PublicationDate: listing.PublicationDate,
                Amenities: listing.Amenities?.Select(AmenityDto.FromDomainModel).ToList() ?? new List<AmenityDto>(),
                Reviews: listing.Reviews?.Select(ReviewDto.FromDomainModel).ToList() ?? new List<ReviewDto>(),
                ListingImages: listing.ListingImages?.Select(ListingImageDto.FromDomainModel).ToList() ?? new List<ListingImageDto>(),
                FavoriteCount: listing.Favorites?.Count ?? 0
            );
    }

    public record CreateListingDto(
        string Title,
        string Description,
        string Address,
        double Latitude,
        double Longitude,
        float Price,
        ListingEnums.ListingType Type,
        List<ListingEnums.CommunalService> CommunalServices,
        ListingEnums.OwnershipType Owners,
        ListingEnums.NeighbourType Neighbours,
        List<Guid> AmenityIds
    );

    public record UpdateListingDto(
        string? Title,
        string? Description,
        string? Address,
        double? Latitude,
        double? Longitude,
        float? Price,
        ListingEnums.ListingType? Type,
        List<ListingEnums.CommunalService>? CommunalServices,
        ListingEnums.OwnershipType? Owners,
        ListingEnums.NeighbourType? Neighbours,
        List<Guid>? AmenityIds
    );
}