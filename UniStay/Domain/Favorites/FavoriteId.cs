namespace Domain.Favorites
{
    public record FavoriteId(Guid Value)
    {
        public static FavoriteId New() => new(Guid.NewGuid());
        public static FavoriteId Empty() => new(Guid.Empty);
        public override string ToString() => Value.ToString();
    }
}