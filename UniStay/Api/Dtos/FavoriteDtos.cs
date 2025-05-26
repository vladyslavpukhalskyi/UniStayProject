using Domain.Favorites;
using Domain.Listings;
using Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Dtos
{
    public record ListingSummaryDto(
        Guid Id,
        string Title)
    {
        public static ListingSummaryDto FromDomainModel(Listing listing) =>
            new(
                Id: listing.Id.Value,
                Title: listing.Title
            );
    }

    public record FavoriteDto(
        Guid Id,
        Guid ListingId,
        ListingSummaryDto? Listing,
        List<UserSummaryDto> Users
    )
    {
        public static FavoriteDto FromDomainModel(Favorite favorite) =>
            new(
                Id: favorite.Id.Value,
                ListingId: favorite.ListingId.Value,
                Listing: favorite.Listing == null ? null : ListingSummaryDto.FromDomainModel(favorite.Listing),
                Users: favorite.Users?.Select(UserSummaryDto.FromDomainModel).ToList() ?? new List<UserSummaryDto>()
            );
    }

    public record CreateFavoriteDto(
        Guid ListingId
    );
}