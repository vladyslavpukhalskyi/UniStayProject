using Application.Listings.Services;
using Domain.Listings;
using System;
using System.Collections.Generic;

namespace Api.Dtos
{
    public record ListingComparisonDto(
        ListingDto Listing1,
        ListingDto Listing2,
        PriceComparisonDto PriceComparison,
        LocationComparisonDto LocationComparison,
        TypeComparisonDto TypeComparison,
        CommunalServicesComparisonDto CommunalServicesComparison,
        OwnershipComparisonDto OwnershipComparison,
        NeighbourComparisonDto NeighbourComparison,
        PublicationDateComparisonDto PublicationDateComparison,
        AmenitiesComparisonDto AmenitiesComparison,
        ReviewsComparisonDto ReviewsComparison,
        ImagesComparisonDto ImagesComparison,
        FavoritesComparisonDto FavoritesComparison
    );

    public record PriceComparisonDto(
        float Listing1Price,
        float Listing2Price,
        ComparisonResult Cheaper,
        float PriceDifference,
        float PriceDifferencePercentage
    );

    public record LocationComparisonDto(
        double Listing1Latitude,
        double Listing1Longitude,
        double Listing2Latitude,
        double Listing2Longitude,
        double DirectDistanceKm,
        List<LandmarkDistanceComparisonDto> LandmarkComparisons
    );

    public record LandmarkDistanceComparisonDto(
        string LandmarkName,
        string LandmarkAddress,
        double Listing1DistanceKm,
        double Listing2DistanceKm,
        ComparisonResult CloserListing,
        double DistanceDifferenceKm
    );

    public record TypeComparisonDto(
        ListingEnums.ListingType Listing1Type,
        ListingEnums.ListingType Listing2Type,
        bool AreSameType
    );

    public record CommunalServicesComparisonDto(
        List<ListingEnums.CommunalService> Listing1Services,
        List<ListingEnums.CommunalService> Listing2Services,
        bool HasIncluded1,
        bool HasIncluded2,
        bool HasSeparate1,
        bool HasSeparate2
    );

    public record OwnershipComparisonDto(
        ListingEnums.OwnershipType Listing1Ownership,
        ListingEnums.OwnershipType Listing2Ownership,
        bool AreSame
    );

    public record NeighbourComparisonDto(
        ListingEnums.NeighbourType Listing1Neighbours,
        ListingEnums.NeighbourType Listing2Neighbours,
        bool AreSame
    );

    public record PublicationDateComparisonDto(
        DateTime Listing1Date,
        DateTime Listing2Date,
        ComparisonResult NewerListing,
        int DaysDifference
    );

    public record AmenitiesComparisonDto(
        int Listing1Count,
        int Listing2Count,
        ComparisonResult MoreAmenities,
        List<string> CommonAmenities,
        List<string> OnlyInListing1,
        List<string> OnlyInListing2
    );

    public record ReviewsComparisonDto(
        int Listing1Count,
        int Listing2Count,
        ComparisonResult MoreReviews,
        double? Listing1AverageRating,
        double? Listing2AverageRating,
        ComparisonResult BetterRated
    );

    public record ImagesComparisonDto(
        int Listing1Count,
        int Listing2Count,
        ComparisonResult MoreImages
    );

    public record FavoritesComparisonDto(
        int Listing1Count,
        int Listing2Count,
        ComparisonResult MorePopular
    );
}
