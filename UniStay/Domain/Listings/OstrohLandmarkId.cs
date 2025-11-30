namespace Domain.Listings
{
    public record OstrohLandmarkId(Guid Value)
    {
        public static OstrohLandmarkId New() => new(Guid.NewGuid());
        public static OstrohLandmarkId Empty() => new(Guid.Empty);
    }
}
