using Domain.Users;
using Domain.Amenities;
using Domain.Favorites;
using Domain.Reviews;
using Domain.ListingImages;

namespace Domain.Listings
{
    public class Listing
    {
        public ListingId Id { get; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string Address { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public float Price { get; private set; }
        public ListingEnums.ListingType Type { get; private set; }
        public UserId UserId { get; private set; }
        public User? User { get; }
        public List<ListingEnums.CommunalService> CommunalServices { get; private set; } = new();
        public ListingEnums.OwnershipType Owners { get; private set; }
        public ListingEnums.NeighbourType Neighbours { get; private set; }
        public DateTime PublicationDate { get; private set; }
        public List<Amenity> Amenities { get; private set; } = new();
        public List<Review> Reviews { get; private set; } = new();
        public List<ListingImage> ListingImages { get; private set; } = new();
        public List<Favorite> Favorites { get; private set; } = new();
        
        private Listing(ListingId id, string title, string description, string address, double latitude, double longitude,
                        float price, ListingEnums.ListingType type, UserId userId, List<ListingEnums.CommunalService> communalServices,
                        ListingEnums.OwnershipType owners, ListingEnums.NeighbourType neighbours, DateTime publicationDate)
        {
            Id = id;
            Title = title;
            Description = description;
            Address = address;
            Latitude = latitude;
            Longitude = longitude;
            Price = price;
            Type = type;
            UserId = userId;
            CommunalServices = communalServices;
            Owners = owners;
            Neighbours = neighbours;
            PublicationDate = publicationDate;
        }

        public static Listing New(ListingId id, string title, string description, string address, double latitude, double longitude,
                                  float price, ListingEnums.ListingType type, UserId userId, List<ListingEnums.CommunalService> communalServices,
                                  ListingEnums.OwnershipType owners, ListingEnums.NeighbourType neighbours, DateTime publicationDate)
            => new(id, title, description, address, latitude, longitude, price, type, userId, communalServices, owners, neighbours, publicationDate);

        public void UpdateDetails(string title, string description, string address, double latitude, double longitude,
                                  float price, ListingEnums.ListingType type, List<ListingEnums.CommunalService> communalServices,
                                  ListingEnums.OwnershipType owners, ListingEnums.NeighbourType neighbours)
        {
            Title = title;
            Description = description;
            Address = address;
            Latitude = latitude;
            Longitude = longitude;
            Price = price;
            Type = type;
            CommunalServices = communalServices;
            Owners = owners;
            Neighbours = neighbours;
        }

        public void AddAmenity(Amenity amenity)
        {
            if (!Amenities.Contains(amenity))
            {
                Amenities.Add(amenity);
            }
        }

        public void RemoveAmenity(Amenity amenity)
        {
            Amenities.Remove(amenity);
        }

        public void AddReview(Review review)
        {
            if (!Reviews.Contains(review))
            {
                Reviews.Add(review);
            }
        }

        public void RemoveReview(Review review)
        {
            Reviews.Remove(review);
        }

        public void AddListingImage(ListingImage listingImage)
        {
            if (!ListingImages.Contains(listingImage))
            {
                ListingImages.Add(listingImage);
            }
        }

        public void RemoveListingImage(ListingImage listingImage)
        {
            ListingImages.Remove(listingImage);
        }

        public void AddFavorite(Favorite favorite)
        {
            if (!Favorites.Contains(favorite))
            {
                Favorites.Add(favorite);
            }
        }

        public void RemoveFavorite(Favorite favorite)
        {
            Favorites.Remove(favorite);
        }
    }
}
