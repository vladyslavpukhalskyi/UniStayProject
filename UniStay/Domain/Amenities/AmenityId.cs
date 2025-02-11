namespace Domain.Amenities
{
    public record AmenityId(Guid Value)
    {
        public static AmenityId New() => new(Guid.NewGuid());
        public static AmenityId Empty() => new(Guid.Empty);
        public override string ToString() => Value.ToString();
    }
}