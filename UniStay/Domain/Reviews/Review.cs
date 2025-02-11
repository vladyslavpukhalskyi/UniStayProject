using Domain.Users;
using Domain.Listings;

namespace Domain.Reviews
{
    public class Review
    {
        public ReviewId Id { get; }
        public UserId UserId { get; private set; }
        
        public User? User { get; }
        public ListingId ListingId { get; private set; }
        
        public Listing? Listing { get; }
        public int Rating { get; private set; }
        public string Comment { get; private set; }
        public DateTime PublicationDate { get; private set; }

        private Review(ReviewId id, UserId userId, ListingId listingId, int rating, string comment, DateTime publicationDate)
        {
            Id = id;
            UserId = userId;
            ListingId = listingId;
            Rating = rating;
            Comment = comment;
            PublicationDate = publicationDate;
        }

        public static Review New(ReviewId id, UserId userId, ListingId listingId, int rating, string comment)
            => new(id, userId, listingId, rating, comment, DateTime.UtcNow);

        public void UpdateReview(int rating, string comment)
        {
            Rating = rating;
            Comment = comment;
            PublicationDate = DateTime.UtcNow;
        }
    }
}