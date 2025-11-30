using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Domain.Users;
using Domain.Amenities;
using Domain.Listings;
using Domain.Reviews;
using Domain.ListingImages;
using Domain.Favorites;
using Application.Common.Interfaces.Auth;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContextInitialiser
    {
        private readonly ILogger<ApplicationDbContextInitialiser> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHash _passwordHasher;

        public ApplicationDbContextInitialiser(
            ILogger<ApplicationDbContextInitialiser> logger,
            ApplicationDbContext context,
            IPasswordHash passwordHasher)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public async Task InitializeAsync()
        {
            try
            {
                if (await _context.Database.CanConnectAsync())
                {
                    _logger.LogInformation("Attempting to apply database migrations.");
                    await _context.Database.MigrateAsync();
                    _logger.LogInformation("Database migrations applied successfully (or no migrations needed).");
                }
                else
                {
                    _logger.LogError("Could not connect to the database. Migrations will not be applied.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initialising the database.");
                throw;
            }
        }

        public async Task SeedAsync()
        {
            try
            {
                await SeedUsersAsync();
                await SeedAmenitiesAsync();
                await SeedListingsAsync();
                await SeedReviewsAsync();
                await SeedListingImagesAsync();
                await SeedFavoritesAsync();
                
                await _context.SaveChangesAsync();
                _logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        private async Task SeedUsersAsync()
        {
            if (!_context.Users.Any())
            {
                _logger.LogInformation("Seeding users...");

                var users = new List<User>
                {
                    // Admin user
                    User.New(
                        UserId.New(),
                        "Admin",
                        "Administrator",
                        "admin@unistay.com",
                        _passwordHasher.HashPassword("Admin123!"),
                        UserEnums.UserRole.Administrator,
                        "+380501234567",
                        "https://example.com/admin-avatar.jpg"
                    ),
                    
                    // Regular user 1
                    User.New(
                        UserId.New(),
                        "Олексій",
                        "Петренко",
                        "oleksiy.petrenko@gmail.com",
                        _passwordHasher.HashPassword("User123!"),
                        UserEnums.UserRole.User,
                        "+380671234567",
                        "https://example.com/user1-avatar.jpg"
                    ),
                    
                    // Regular user 2
                    User.New(
                        UserId.New(),
                        "Марія",
                        "Іваненко",
                        "maria.ivanenko@gmail.com",
                        _passwordHasher.HashPassword("User123!"),
                        UserEnums.UserRole.User,
                        "+380931234567",
                        "https://example.com/user2-avatar.jpg"
                    )
                };

                _context.Users.AddRange(users);
                _logger.LogInformation($"Seeded {users.Count} users.");
            }
        }

        private async Task SeedAmenitiesAsync()
        {
            if (!_context.Amenities.Any())
            {
                _logger.LogInformation("Seeding amenities...");

                var amenities = new List<Amenity>
                {
                    Amenity.New(AmenityId.New(), "Wi-Fi"),
                    Amenity.New(AmenityId.New(), "Кондиціонер"),
                    Amenity.New(AmenityId.New(), "Пральна машина"),
                    Amenity.New(AmenityId.New(), "Холодильник"),
                    Amenity.New(AmenityId.New(), "Телевізор")
                };

                _context.Amenities.AddRange(amenities);
                _logger.LogInformation($"Seeded {amenities.Count} amenities.");
            }
        }

        private async Task SeedListingsAsync()
        {
            if (!_context.Listings.Any())
            {
                _logger.LogInformation("Seeding listings...");

                var users = await _context.Users.ToListAsync();
                var regularUsers = users.Where(u => u.Role == UserEnums.UserRole.User).ToList();

                if (regularUsers.Any())
                {
                    var listings = new List<Listing>
                    {
                        Listing.New(
                            ListingId.New(),
                            "Затишна квартира в центрі Києва",
                            "Прекрасна 2-кімнатна квартира з сучасним ремонтом та всіма зручностями. Розташована в самому центрі міста, поруч з метро.",
                            "вул. Хрещатик, 15, Київ",
                            50.4501,
                            30.5234,
                            15000.0f,
                            ListingEnums.ListingType.Apartment,
                            regularUsers[0].Id,
                            new List<ListingEnums.CommunalService> { ListingEnums.CommunalService.Included },
                            ListingEnums.OwnershipType.Without,
                            ListingEnums.NeighbourType.With,
                            DateTime.UtcNow.AddDays(-10)
                        ),
                        
                        Listing.New(
                            ListingId.New(),
                            "Студентська кімната біля НаУКМА",
                            "Комфортна кімната для студента з усім необхідним. Швидкий інтернет, тиха обстановка для навчання.",
                            "вул. Сковороди, 2, Київ",
                            50.4648,
                            30.5107,
                            8000.0f,
                            ListingEnums.ListingType.Room,
                            regularUsers.Count > 1 ? regularUsers[1].Id : regularUsers[0].Id,
                            new List<ListingEnums.CommunalService> { ListingEnums.CommunalService.Separate },
                            ListingEnums.OwnershipType.With,
                            ListingEnums.NeighbourType.With,
                            DateTime.UtcNow.AddDays(-5)
                        ),
                        
                        Listing.New(
                            ListingId.New(),
                            "Приватний будинок для сім'ї",
                            "Просторий 3-поверховий будинок з великим садом. Ідеально підходить для великої сім'ї або групи студентів.",
                            "вул. Садова, 45, Київ",
                            50.4265,
                            30.5383,
                            25000.0f,
                            ListingEnums.ListingType.House,
                            regularUsers[0].Id,
                            new List<ListingEnums.CommunalService> { ListingEnums.CommunalService.Included },
                            ListingEnums.OwnershipType.Without,
                            ListingEnums.NeighbourType.Without,
                            DateTime.UtcNow.AddDays(-3)
                        ),
                        
                        Listing.New(
                            ListingId.New(),
                            "Сучасна квартира в Печерську",
                            "Нова квартира в елітному районі з панорамним видом на Дніпро. Всі меблі та техніка включені.",
                            "вул. Липська, 10, Київ",
                            50.4265,
                            30.5383,
                            20000.0f,
                            ListingEnums.ListingType.Apartment,
                            regularUsers.Count > 1 ? regularUsers[1].Id : regularUsers[0].Id,
                            new List<ListingEnums.CommunalService> { ListingEnums.CommunalService.Included },
                            ListingEnums.OwnershipType.Without,
                            ListingEnums.NeighbourType.With,
                            DateTime.UtcNow.AddDays(-1)
                        ),
                        
                        Listing.New(
                            ListingId.New(),
                            "Економ-кімната для студента",
                            "Бюджетний варіант для студента. Основні зручності є, хороше транспортне сполучення.",
                            "вул. Політехнічна, 6, Київ",
                            50.4501,
                            30.4721,
                            5500.0f,
                            ListingEnums.ListingType.Room,
                            regularUsers[0].Id,
                            new List<ListingEnums.CommunalService> { ListingEnums.CommunalService.Separate },
                            ListingEnums.OwnershipType.With,
                            ListingEnums.NeighbourType.With,
                            DateTime.UtcNow
                        )
                    };

                    _context.Listings.AddRange(listings);
                    _logger.LogInformation($"Seeded {listings.Count} listings.");
                }
            }
        }

        private async Task SeedReviewsAsync()
        {
            if (!_context.Reviews.Any())
            {
                _logger.LogInformation("Seeding reviews...");

                var users = await _context.Users.Where(u => u.Role == UserEnums.UserRole.User).ToListAsync();
                var listings = await _context.Listings.ToListAsync();

                if (users.Any() && listings.Any())
                {
                    var reviews = new List<Review>
                    {
                        Review.New(
                            ReviewId.New(),
                            users[0].Id,
                            listings[0].Id,
                            5,
                            "Чудове місце! Дуже чисто та затишно. Господар дуже привітний та завжди готовий допомогти."
                        ),
                        
                        Review.New(
                            ReviewId.New(),
                            users.Count > 1 ? users[1].Id : users[0].Id,
                            listings.Count > 1 ? listings[1].Id : listings[0].Id,
                            4,
                            "Гарна кімната, все як на фото. Єдиний мінус - трохи шумно від сусідів."
                        ),
                        
                        Review.New(
                            ReviewId.New(),
                            users[0].Id,
                            listings.Count > 2 ? listings[2].Id : listings[0].Id,
                            5,
                            "Ідеальний будинок для нашої групи! Багато місця, всі зручності є."
                        ),
                        
                        Review.New(
                            ReviewId.New(),
                            users.Count > 1 ? users[1].Id : users[0].Id,
                            listings.Count > 3 ? listings[3].Id : listings[0].Id,
                            4,
                            "Дуже гарна квартира з чудовим видом. Рекомендую!"
                        ),
                        
                        Review.New(
                            ReviewId.New(),
                            users[0].Id,
                            listings.Count > 4 ? listings[4].Id : listings[0].Id,
                            3,
                            "Нормальне місце за свою ціну. Для студента підходить."
                        )
                    };

                    _context.Reviews.AddRange(reviews);
                    _logger.LogInformation($"Seeded {reviews.Count} reviews.");
                }
            }
        }

        private async Task SeedListingImagesAsync()
        {
            if (!_context.ListingImages.Any())
            {
                _logger.LogInformation("Seeding listing images...");

                var listings = await _context.Listings.ToListAsync();

                if (listings.Any())
                {
                    var listingImages = new List<ListingImage>();

                    foreach (var listing in listings.Take(5))
                    {
                        listingImages.Add(ListingImage.New(
                            ListingImageId.New(),
                            listing.Id,
                            $"https://example.com/listing-{listing.Id.Value}-1.jpg"
                        ));
                        
                        listingImages.Add(ListingImage.New(
                            ListingImageId.New(),
                            listing.Id,
                            $"https://example.com/listing-{listing.Id.Value}-2.jpg"
                        ));
                    }

                    _context.ListingImages.AddRange(listingImages);
                    _logger.LogInformation($"Seeded {listingImages.Count} listing images.");
                }
            }
        }

        private async Task SeedFavoritesAsync()
        {
            if (!_context.Favorites.Any())
            {
                _logger.LogInformation("Seeding favorites...");

                var users = await _context.Users.Where(u => u.Role == UserEnums.UserRole.User).ToListAsync();
                var listings = await _context.Listings.ToListAsync();

                if (users.Any() && listings.Any())
                {
                    var favorites = new List<Favorite>
                    {
                        Favorite.New(FavoriteId.New(), listings[0].Id),
                        Favorite.New(FavoriteId.New(), listings.Count > 1 ? listings[1].Id : listings[0].Id),
                        Favorite.New(FavoriteId.New(), listings.Count > 2 ? listings[2].Id : listings[0].Id),
                        Favorite.New(FavoriteId.New(), listings.Count > 3 ? listings[3].Id : listings[0].Id),
                        Favorite.New(FavoriteId.New(), listings.Count > 4 ? listings[4].Id : listings[0].Id)
                    };

                    _context.Favorites.AddRange(favorites);
                    _logger.LogInformation($"Seeded {favorites.Count} favorites.");
                }
            }
        }
    }
}