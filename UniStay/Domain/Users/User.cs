using Domain.Favorites;
using Domain.Listings;
using Domain.Reviews;
using Domain.Messages;

namespace Domain.Users
{
    public class User
    {
        public UserId Id { get; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public string Password { get; private set; }
        public UserEnums.UserRole Role { get; private set; }
        public string PhoneNumber { get; private set; }
        public string ProfileImage { get; private set; }
        public DateTime RegistrationDate { get; }
        
        public List<Favorite> Favorites { get; private set; } = new();
        
        public List<Listing> Listings { get; private set; } = new();
        
        public List<Review> Reviews { get; private set; } = new();
        
        public List<Message> SentMessages { get; private set; } = new();
        
        public List<Message> ReceivedMessages { get; private set; } = new();

        public string FullName => $"{FirstName} {LastName}";

        private User(UserId id, string firstName, string lastName, string email, string password, UserEnums.UserRole role,
            string phoneNumber, string profileImage, DateTime registrationDate)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Password = password;
            Role = role;
            PhoneNumber = phoneNumber;
            ProfileImage = profileImage;
            RegistrationDate = registrationDate;
        }

        public static User New(UserId id, string firstName, string lastName, string email, string password,
            UserEnums.UserRole role, string phoneNumber, string profileImage)
            => new(id, firstName, lastName, email, password, role, phoneNumber, profileImage, DateTime.UtcNow);

        public void UpdateDetails(string firstName, string lastName, string phoneNumber, string profileImage)
        {
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            ProfileImage = profileImage;
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
            if (Favorites.Contains(favorite))
            {
                Favorites.Remove(favorite);
            }
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
            if (Reviews.Contains(review))
            {
                Reviews.Remove(review);
            }
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
        
        public void AddSentMessage(Message message)
        {
            if (!SentMessages.Contains(message))
            {
                SentMessages.Add(message);
            }
        }
        
        public void AddReceivedMessage(Message message)
        {
            if (!ReceivedMessages.Contains(message))
            {
                ReceivedMessages.Add(message);
            }
        }
        
        public void RemoveSentMessage(Message message)
        {
            if (SentMessages.Contains(message))
            {
                SentMessages.Remove(message);
            }
        }
        
        public void RemoveReceivedMessage(Message message)
        {
            if (ReceivedMessages.Contains(message))
            {
                ReceivedMessages.Remove(message);
            }
        }
    }
}


//userrrr
