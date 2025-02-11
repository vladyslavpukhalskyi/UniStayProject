namespace Domain.Reviews
{
    public record ReviewId(Guid Value)
    {
        public static ReviewId New() => new(Guid.NewGuid());
        public static ReviewId Empty() => new(Guid.Empty);
        public override string ToString() => Value.ToString();
    }
}