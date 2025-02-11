namespace Domain.ListingImages
{
    public record ListingImageId(Guid Value)
    {
        public static ListingImageId New() => new(Guid.NewGuid());
        public static ListingImageId Empty() => new(Guid.Empty);
        public override string ToString() => Value.ToString();
    }
}