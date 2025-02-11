namespace Domain.Listings
{
    public record ListingId(Guid Value)
    {
        public static ListingId New() => new(Guid.NewGuid());
        public static ListingId Empty() => new(Guid.Empty);
        public override string ToString() => Value.ToString();
    }
}