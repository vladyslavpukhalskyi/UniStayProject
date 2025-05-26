using Domain.ListingImages;
using Domain.Listings;
using System;

namespace Api.Dtos
{
    public record ListingImageDto(
        Guid Id,
        Guid ListingId,
        string ImageUrl)
    {
        public static ListingImageDto FromDomainModel(ListingImage image) =>
            new(
                Id: image.Id.Value,
                ListingId: image.ListingId.Value,
                ImageUrl: image.ImageUrl
            );
    }

    public record CreateListingImageDto(
        Guid ListingId,
        string ImageUrl
    );

    public record UpdateListingImageDto(
        string ImageUrl
    );
}