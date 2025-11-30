using Api.Dtos.Listing;
using Application.Listings.Services;
using Domain.Listings;
using Application.Common.Interfaces.Queries;
using Application.Listings.Queries;

namespace Api.Dtos.Listing
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

public interface IListingComparisonDtoMapper
{
    Task<ListingComparisonDto> MapToDto(ListingComparisonResult result, CancellationToken cancellationToken);
}

public class ListingComparisonDtoMapper : IListingComparisonDtoMapper
{
    private readonly IListingComparisonService _comparisonService;
    private readonly IOstrohLandmarksQueries _landmarksQueries;

    public ListingComparisonDtoMapper(
        IListingComparisonService comparisonService,
        IOstrohLandmarksQueries landmarksQueries)
    {
        _comparisonService = comparisonService ?? throw new ArgumentNullException(nameof(comparisonService));
        _landmarksQueries = landmarksQueries ?? throw new ArgumentNullException(nameof(landmarksQueries));
    }

    public async Task<ListingComparisonDto> MapToDto(ListingComparisonResult result,
        CancellationToken cancellationToken)
    {
        var listing1 = result.Listing1;
        var listing2 = result.Listing2;

        var landmarks = await _landmarksQueries.GetAll(cancellationToken);

        var priceComp = _comparisonService.ComparePrices(listing1, listing2);
        var locationComp = _comparisonService.CompareLocations(listing1, listing2, landmarks);
        var typeComp = _comparisonService.CompareTypes(listing1, listing2);
        var communalComp = _comparisonService.CompareCommunalServices(listing1, listing2);
        var ownershipComp = _comparisonService.CompareOwnership(listing1, listing2);
        var neighbourComp = _comparisonService.CompareNeighbours(listing1, listing2);
        var dateComp = _comparisonService.ComparePublicationDates(listing1, listing2);
        var amenitiesComp = _comparisonService.CompareAmenities(listing1, listing2);
        var reviewsComp = _comparisonService.CompareReviews(listing1, listing2);
        var imagesComp = _comparisonService.CompareImages(listing1, listing2);
        var favoritesComp = _comparisonService.CompareFavorites(listing1, listing2);

        return new ListingComparisonDto(
            Listing1: ListingDto.FromDomainModel(listing1),
            Listing2: ListingDto.FromDomainModel(listing2),
            PriceComparison: new PriceComparisonDto(priceComp.Listing1Price, priceComp.Listing2Price, priceComp.Cheaper,
                priceComp.PriceDifference, priceComp.PriceDifferencePercentage),
            LocationComparison: new LocationComparisonDto(locationComp.Listing1Latitude, locationComp.Listing1Longitude,
                locationComp.Listing2Latitude, locationComp.Listing2Longitude, locationComp.DirectDistanceKm,
                locationComp.LandmarkComparisons.Select(l => new LandmarkDistanceComparisonDto(l.LandmarkName,
                    l.LandmarkAddress, l.Listing1DistanceKm, l.Listing2DistanceKm, l.CloserListing,
                    l.DistanceDifferenceKm)).ToList()),
            TypeComparison: new TypeComparisonDto(typeComp.Listing1Type, typeComp.Listing2Type, typeComp.AreSameType),
            CommunalServicesComparison: new CommunalServicesComparisonDto(communalComp.Listing1Services,
                communalComp.Listing2Services, communalComp.HasIncluded1, communalComp.HasIncluded2,
                communalComp.HasSeparate1, communalComp.HasSeparate2),
            OwnershipComparison: new OwnershipComparisonDto(ownershipComp.Listing1Ownership,
                ownershipComp.Listing2Ownership, ownershipComp.AreSame),
            NeighbourComparison: new NeighbourComparisonDto(neighbourComp.Listing1Neighbours,
                neighbourComp.Listing2Neighbours, neighbourComp.AreSame),
            PublicationDateComparison: new PublicationDateComparisonDto(dateComp.Listing1Date, dateComp.Listing2Date,
                dateComp.NewerListing, dateComp.DaysDifference),
            AmenitiesComparison: new AmenitiesComparisonDto(amenitiesComp.Listing1Count, amenitiesComp.Listing2Count,
                amenitiesComp.MoreAmenities, amenitiesComp.CommonAmenities, amenitiesComp.OnlyInListing1,
                amenitiesComp.OnlyInListing2),
            ReviewsComparison: new ReviewsComparisonDto(reviewsComp.Listing1Count, reviewsComp.Listing2Count,
                reviewsComp.MoreReviews, reviewsComp.Listing1AverageRating, reviewsComp.Listing2AverageRating,
                reviewsComp.BetterRated),
            ImagesComparison: new ImagesComparisonDto(imagesComp.Listing1Count, imagesComp.Listing2Count,
                imagesComp.MoreImages),
            FavoritesComparison: new FavoritesComparisonDto(favoritesComp.Listing1Count, favoritesComp.Listing2Count,
                favoritesComp.MorePopular)
        );
    }
}