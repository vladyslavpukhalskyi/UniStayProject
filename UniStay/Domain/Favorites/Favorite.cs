using Domain.Listings;
using Domain.Users;

namespace Domain.Favorites
{
    public class Favorite
    {
        public FavoriteId Id { get; private set; }
        public ListingId ListingId { get; private set; }
        public Listing? Listing { get; private set; }  
        public List<User> Users { get; private set; } = new();

        private Favorite() { }

        private Favorite(FavoriteId id, ListingId listingId)
        {
            Id = id;
            ListingId = listingId;
        }

        public static Favorite New(FavoriteId id, ListingId listingId)
            => new(id, listingId);

        public void AddUser(User user)
        {
            if (!Users.Contains(user))
            {
                Users.Add(user);
            }
        }

        public void RemoveUser(User user)
        {
            if (Users.Contains(user))
            {
                Users.Remove(user);
            }
        }
    }
}