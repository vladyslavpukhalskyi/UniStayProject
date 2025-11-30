using Domain.Listings;

namespace Application.Listings.Services
{
    public interface IListingComparisonService
    {
        PriceComparison ComparePrices(Listing listing1, Listing listing2);
        LocationComparison CompareLocations(Listing listing1, Listing listing2, IReadOnlyList<OstrohLandmark> landmarks);
        TypeComparison CompareTypes(Listing listing1, Listing listing2);
        CommunalServicesComparison CompareCommunalServices(Listing listing1, Listing listing2);
        OwnershipComparison CompareOwnership(Listing listing1, Listing listing2);
        NeighbourComparison CompareNeighbours(Listing listing1, Listing listing2);
        PublicationDateComparison ComparePublicationDates(Listing listing1, Listing listing2);
        AmenitiesComparison CompareAmenities(Listing listing1, Listing listing2);
        ReviewsComparison CompareReviews(Listing listing1, Listing listing2);
        ImagesComparison CompareImages(Listing listing1, Listing listing2);
        FavoritesComparison CompareFavorites(Listing listing1, Listing listing2);
    }

    public class ListingComparisonService : IListingComparisonService
    {
        public PriceComparison ComparePrices(Listing listing1, Listing listing2)
        {
            var priceDiff = Math.Abs(listing1.Price - listing2.Price);
            var cheaper = listing1.Price < listing2.Price ? ComparisonResult.Listing1 :
                         listing1.Price > listing2.Price ? ComparisonResult.Listing2 : ComparisonResult.Same;
            var percentage = listing2.Price != 0
                ? (priceDiff / listing2.Price) * 100
                : 0;

            return new PriceComparison(
                Listing1Price: listing1.Price,
                Listing2Price: listing2.Price,
                Cheaper: cheaper,
                PriceDifference: priceDiff,
                PriceDifferencePercentage: percentage
            );
        }

        public LocationComparison CompareLocations(Listing listing1, Listing listing2, IReadOnlyList<OstrohLandmark> landmarks)
        {
            var directDistance = CalculateDistance(
                listing1.Latitude, listing1.Longitude,
                listing2.Latitude, listing2.Longitude
            );

            var landmarkComparisons = landmarks.Select(landmark =>
            {
                var dist1 = landmark.CalculateDistanceTo(listing1.Latitude, listing1.Longitude);
                var dist2 = landmark.CalculateDistanceTo(listing2.Latitude, listing2.Longitude);
                var closer = dist1 < dist2 ? ComparisonResult.Listing1 :
                            dist1 > dist2 ? ComparisonResult.Listing2 : ComparisonResult.Same;

                return new LandmarkDistanceComparison(
                    LandmarkName: landmark.Name,
                    LandmarkAddress: landmark.Address,
                    Listing1DistanceKm: Math.Round(dist1, 3),
                    Listing2DistanceKm: Math.Round(dist2, 3),
                    CloserListing: closer,
                    DistanceDifferenceKm: Math.Round(Math.Abs(dist1 - dist2), 3)
                );
            }).ToList();

            return new LocationComparison(
                Listing1Latitude: listing1.Latitude,
                Listing1Longitude: listing1.Longitude,
                Listing2Latitude: listing2.Latitude,
                Listing2Longitude: listing2.Longitude,
                DirectDistanceKm: Math.Round(directDistance, 3),
                LandmarkComparisons: landmarkComparisons
            );
        }

        public TypeComparison CompareTypes(Listing listing1, Listing listing2)
        {
            return new TypeComparison(
                Listing1Type: listing1.Type,
                Listing2Type: listing2.Type,
                AreSameType: listing1.Type == listing2.Type
            );
        }

        public CommunalServicesComparison CompareCommunalServices(Listing listing1, Listing listing2)
        {
            var hasIncluded1 = listing1.CommunalServices.Contains(ListingEnums.CommunalService.Included);
            var hasIncluded2 = listing2.CommunalServices.Contains(ListingEnums.CommunalService.Included);
            var hasSeparate1 = listing1.CommunalServices.Contains(ListingEnums.CommunalService.Separate);
            var hasSeparate2 = listing2.CommunalServices.Contains(ListingEnums.CommunalService.Separate);

            return new CommunalServicesComparison(
                Listing1Services: listing1.CommunalServices,
                Listing2Services: listing2.CommunalServices,
                HasIncluded1: hasIncluded1,
                HasIncluded2: hasIncluded2,
                HasSeparate1: hasSeparate1,
                HasSeparate2: hasSeparate2
            );
        }

        public OwnershipComparison CompareOwnership(Listing listing1, Listing listing2)
        {
            return new OwnershipComparison(
                Listing1Ownership: listing1.Owners,
                Listing2Ownership: listing2.Owners,
                AreSame: listing1.Owners == listing2.Owners
            );
        }

        public NeighbourComparison CompareNeighbours(Listing listing1, Listing listing2)
        {
            return new NeighbourComparison(
                Listing1Neighbours: listing1.Neighbours,
                Listing2Neighbours: listing2.Neighbours,
                AreSame: listing1.Neighbours == listing2.Neighbours
            );
        }

        public PublicationDateComparison ComparePublicationDates(Listing listing1, Listing listing2)
        {
            var daysDiff = Math.Abs((listing1.PublicationDate - listing2.PublicationDate).Days);
            var newer = listing1.PublicationDate > listing2.PublicationDate ? ComparisonResult.Listing1 :
                       listing1.PublicationDate < listing2.PublicationDate ? ComparisonResult.Listing2 : ComparisonResult.Same;

            return new PublicationDateComparison(
                Listing1Date: listing1.PublicationDate,
                Listing2Date: listing2.PublicationDate,
                NewerListing: newer,
                DaysDifference: daysDiff
            );
        }

        public AmenitiesComparison CompareAmenities(Listing listing1, Listing listing2)
        {
            var amenities1 = listing1.Amenities.Select(a => a.Title).ToHashSet();
            var amenities2 = listing2.Amenities.Select(a => a.Title).ToHashSet();

            var common = amenities1.Intersect(amenities2).ToList();
            var onlyIn1 = amenities1.Except(amenities2).ToList();
            var onlyIn2 = amenities2.Except(amenities1).ToList();

            var moreAmenities = listing1.Amenities.Count > listing2.Amenities.Count ? ComparisonResult.Listing1 :
                               listing1.Amenities.Count < listing2.Amenities.Count ? ComparisonResult.Listing2 : ComparisonResult.Same;

            return new AmenitiesComparison(
                Listing1Count: listing1.Amenities.Count,
                Listing2Count: listing2.Amenities.Count,
                MoreAmenities: moreAmenities,
                CommonAmenities: common,
                OnlyInListing1: onlyIn1,
                OnlyInListing2: onlyIn2
            );
        }

        public ReviewsComparison CompareReviews(Listing listing1, Listing listing2)
        {
            var moreReviews = listing1.Reviews.Count > listing2.Reviews.Count ? ComparisonResult.Listing1 :
                             listing1.Reviews.Count < listing2.Reviews.Count ? ComparisonResult.Listing2 : ComparisonResult.Same;

            double? avg1 = listing1.Reviews.Any() ? listing1.Reviews.Average(r => r.Rating) : null;
            double? avg2 = listing2.Reviews.Any() ? listing2.Reviews.Average(r => r.Rating) : null;

            var betterRated = avg1.HasValue && avg2.HasValue
                ? (avg1.Value > avg2.Value ? ComparisonResult.Listing1 : avg1.Value < avg2.Value ? ComparisonResult.Listing2 : ComparisonResult.Same)
                : (avg1.HasValue ? ComparisonResult.Listing1 : avg2.HasValue ? ComparisonResult.Listing2 : ComparisonResult.None);

            return new ReviewsComparison(
                Listing1Count: listing1.Reviews.Count,
                Listing2Count: listing2.Reviews.Count,
                MoreReviews: moreReviews,
                Listing1AverageRating: avg1.HasValue ? Math.Round(avg1.Value, 2) : null,
                Listing2AverageRating: avg2.HasValue ? Math.Round(avg2.Value, 2) : null,
                BetterRated: betterRated
            );
        }

        public ImagesComparison CompareImages(Listing listing1, Listing listing2)
        {
            var moreImages = listing1.ListingImages.Count > listing2.ListingImages.Count ? ComparisonResult.Listing1 :
                            listing1.ListingImages.Count < listing2.ListingImages.Count ? ComparisonResult.Listing2 : ComparisonResult.Same;

            return new ImagesComparison(
                Listing1Count: listing1.ListingImages.Count,
                Listing2Count: listing2.ListingImages.Count,
                MoreImages: moreImages
            );
        }

        public FavoritesComparison CompareFavorites(Listing listing1, Listing listing2)
        {
            var morePopular = listing1.Favorites.Count > listing2.Favorites.Count ? ComparisonResult.Listing1 :
                             listing1.Favorites.Count < listing2.Favorites.Count ? ComparisonResult.Listing2 : ComparisonResult.Same;

            return new FavoritesComparison(
                Listing1Count: listing1.Favorites.Count,
                Listing2Count: listing2.Favorites.Count,
                MorePopular: morePopular
            );
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadiusKm = 6371.0;
            var lat1Rad = lat1 * Math.PI / 180.0;
            var lat2Rad = lat2 * Math.PI / 180.0;
            var deltaLat = (lat2 - lat1) * Math.PI / 180.0;
            var deltaLon = (lon2 - lon1) * Math.PI / 180.0;

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                    Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusKm * c;
        }
    }

    // Comparison result models
    public enum ComparisonResult
    {
        Listing1,
        Listing2,
        Same,
        None
    }

    public record PriceComparison(
        float Listing1Price,
        float Listing2Price,
        ComparisonResult Cheaper,
        float PriceDifference,
        float PriceDifferencePercentage
    );

    public record LocationComparison(
        double Listing1Latitude,
        double Listing1Longitude,
        double Listing2Latitude,
        double Listing2Longitude,
        double DirectDistanceKm,
        List<LandmarkDistanceComparison> LandmarkComparisons
    );

    public record LandmarkDistanceComparison(
        string LandmarkName,
        string LandmarkAddress,
        double Listing1DistanceKm,
        double Listing2DistanceKm,
        ComparisonResult CloserListing,
        double DistanceDifferenceKm
    );

    public record TypeComparison(
        ListingEnums.ListingType Listing1Type,
        ListingEnums.ListingType Listing2Type,
        bool AreSameType
    );

    public record CommunalServicesComparison(
        List<ListingEnums.CommunalService> Listing1Services,
        List<ListingEnums.CommunalService> Listing2Services,
        bool HasIncluded1,
        bool HasIncluded2,
        bool HasSeparate1,
        bool HasSeparate2
    );

    public record OwnershipComparison(
        ListingEnums.OwnershipType Listing1Ownership,
        ListingEnums.OwnershipType Listing2Ownership,
        bool AreSame
    );

    public record NeighbourComparison(
        ListingEnums.NeighbourType Listing1Neighbours,
        ListingEnums.NeighbourType Listing2Neighbours,
        bool AreSame
    );

    public record PublicationDateComparison(
        DateTime Listing1Date,
        DateTime Listing2Date,
        ComparisonResult NewerListing,
        int DaysDifference
    );

    public record AmenitiesComparison(
        int Listing1Count,
        int Listing2Count,
        ComparisonResult MoreAmenities,
        List<string> CommonAmenities,
        List<string> OnlyInListing1,
        List<string> OnlyInListing2
    );

    public record ReviewsComparison(
        int Listing1Count,
        int Listing2Count,
        ComparisonResult MoreReviews,
        double? Listing1AverageRating,
        double? Listing2AverageRating,
        ComparisonResult BetterRated
    );

    public record ImagesComparison(
        int Listing1Count,
        int Listing2Count,
        ComparisonResult MoreImages
    );

    public record FavoritesComparison(
        int Listing1Count,
        int Listing2Count,
        ComparisonResult MorePopular
    );
}
