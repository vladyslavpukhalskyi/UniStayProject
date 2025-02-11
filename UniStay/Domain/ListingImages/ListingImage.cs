using Domain.Listings;

namespace Domain.ListingImages
{
    public class ListingImage
    {
        public ListingImageId Id { get; }
        public ListingId ListingId { get; private set; }
        
        public Listing? Listing { get; }
        public string ImageUrl { get; private set; }
        
        private ListingImage(ListingImageId id, ListingId listingId, string imageUrl)
        {
            Id = id;
            ListingId = listingId;
            ImageUrl = imageUrl;
        }

        public static ListingImage New(ListingImageId id, ListingId listingId, string imageUrl)
            => new(id, listingId, imageUrl);

        public void UpdateImageUrl(string imageUrl)
        {
            ImageUrl = imageUrl;
        }
    }
}