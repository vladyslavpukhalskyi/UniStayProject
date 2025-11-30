using Domain.Amenities;
using System;

namespace Api.Dtos
{
    public record AmenityDto(
        Guid Id,
        string Title)
    {
        public static AmenityDto FromDomainModel(Amenity amenity) =>
            new(
                Id: amenity.Id.Value,
                Title: amenity.Title
            );
    }

    public record CreateAmenityDto(
        string Title
    );

    public record UpdateAmenityDto(
        string? Title
    );
}