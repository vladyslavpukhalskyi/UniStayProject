using Domain.Listings;
using Domain.Users;
using Domain.Amenities;
using Domain.Reviews;
using Domain.ListingImages;
using Domain.Favorites;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Dtos
{
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
        List<AmenityDto> Amenities,
        List<ReviewDto> Reviews,
        List<ListingImageDto> ListingImages,
        int FavoriteCount
    )
    {
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
        float Price,
        ListingEnums.ListingType Type,
        List<ListingEnums.CommunalService> CommunalServices,
        ListingEnums.OwnershipType Owners,
        ListingEnums.NeighbourType Neighbours,
        List<Guid> AmenityIds
    );

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