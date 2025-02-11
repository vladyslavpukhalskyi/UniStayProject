using Domain.Listings;

namespace Domain.Amenities
{
    public class Amenity
    {
        public AmenityId Id { get; }
        public string Title { get; private set; }
        public List<Listing> Listings { get; private set; } = new List<Listing>();

        private Amenity(AmenityId id, string title)
        {
            Id = id;
            Title = title;
        }

        public static Amenity New(AmenityId id, string title)
            => new(id, title);

        public void UpdateTitle(string title)
        {
            Title = title;
        }

        public void AddListing(Listing listing)
        {
            if (!Listings.Contains(listing))
            {
                Listings.Add(listing);
            }
        }

        public void RemoveListing(Listing listing)
        {
            if (Listings.Contains(listing))
            {
                Listings.Remove(listing);
            }
        }
    }
}