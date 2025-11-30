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
                await SeedOstrohLandmarksAsync();
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

        private async Task SeedOstrohLandmarksAsync()
        {
            if (!_context.OstrohLandmarks.Any())
            {
                _logger.LogInformation("Seeding Ostroh landmarks...");

                var landmarks = new List<OstrohLandmark>
                {
                    OstrohLandmark.Create(OstrohLandmarkId.New(), "Новий корпус Острозької академії", "вул. Семінарська, 2", 50.32917, 26.51278),
                    OstrohLandmark.Create(OstrohLandmarkId.New(), "Старий корпус Острозької академії", "вул. Семінарська, 2", 50.32833, 26.51278),
                    OstrohLandmark.Create(OstrohLandmarkId.New(), "АТБ (Супермаркет)", "вул. Гальшки Острозької, 1в", 50.32729, 26.52463),
                    OstrohLandmark.Create(OstrohLandmarkId.New(), "Острозький замок", "вул. Академічна, 5", 50.32626, 26.52212),
                    OstrohLandmark.Create(OstrohLandmarkId.New(), "Нова Пошта (Відділення №1)", "вул. Князів Острозьких, 3", 50.32849, 26.51955),
                    OstrohLandmark.Create(OstrohLandmarkId.New(), "Укрпошта (Відділення 35800)", "просп. Незалежності, 7", 50.32951, 26.52054),
                    OstrohLandmark.Create(OstrohLandmarkId.New(), "Татарська вежа", "вул. Татарська", 50.3306, 26.5260),
                    OstrohLandmark.Create(OstrohLandmarkId.New(), "Автовокзал \"Острог\"", "просп. Незалежності, 166", 50.33557, 26.49400),
                    OstrohLandmark.Create(OstrohLandmarkId.New(), "Богоявленський собор", "вул. Академічна, 5в", 50.32661, 26.52128),
                    OstrohLandmark.Create(OstrohLandmarkId.New(), "Гуртожиток \"Академічний дім\" (№5)", "просп. Незалежності, 5", 50.32951, 26.52054)
                };

                _context.OstrohLandmarks.AddRange(landmarks);
                _logger.LogInformation($"Seeded {landmarks.Count} Ostroh landmarks.");
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
                            "Кімната для студента біля НаУОА",
                            "Затишна кімната в 5 хвилинах пішки від Острозької академії. Є все необхідне для навчання: стіл, ліжко, шафа, швидкісний інтернет. Cпільна кухня та ванна кімната.",
                            "вул. Семінарська, 4, Острог",
                            50.3292,
                            26.5130,
                            4500.0f,
                            ListingEnums.ListingType.Room,
                            regularUsers[0].Id,
                            new List<ListingEnums.CommunalService> { ListingEnums.CommunalService.Included },
                            ListingEnums.OwnershipType.With,
                            ListingEnums.NeighbourType.With,
                            DateTime.UtcNow.AddDays(-10)
                        ),

                        Listing.New(
                            ListingId.New(),
                            "1-кімнатна квартира в центрі Острога",
                            "Світла квартира на проспекті Незалежності. Поруч кафе 'Маестро' та 'Карамель', до академії 10-15 хвилин. Встановлено бойлер, є пральна машина.",
                            "просп. Незалежності, 10, Острог",
                            50.3300,
                            26.5165,
                            8000.0f,
                            ListingEnums.ListingType.Apartment,
                            regularUsers.Count > 1 ? regularUsers[1].Id : regularUsers[0].Id,
                            new List<ListingEnums.CommunalService> { ListingEnums.CommunalService.Separate },
                            ListingEnums.OwnershipType.Without,
                            ListingEnums.NeighbourType.Without,
                            DateTime.UtcNow.AddDays(-5)
                        ),

                        Listing.New(
                            ListingId.New(),
                            "Окремий будинок для групи студентів",
                            "Здається частина будинку (2 поверхи) в районі 'Нове місто'. Ідеально для 3-4 студентів. Є невеликий власний двір. Окремий вхід.",
                            "вул. Бельмаж, 15, Острог",
                            50.3255,
                            26.5198,
                            12000.0f,
                            ListingEnums.ListingType.House,
                            regularUsers[0].Id,
                            new List<ListingEnums.CommunalService> { ListingEnums.CommunalService.Separate },
                            ListingEnums.OwnershipType.Without,
                            ListingEnums.NeighbourType.Without,
                            DateTime.UtcNow.AddDays(-3)
                        ),

                        Listing.New(
                            ListingId.New(),
                            "2-кімнатна квартира (район мед. коледжу)",
                            "Простора квартира з окремими кімнатами. Поруч Острозький медичний коледж. Повністю мебльована, є холодильник та вся необхідна техніка.",
                            "вул. Карнаухова, 5, Острог",
                            50.3325,
                            26.5080,
                            9500.0f,
                            ListingEnums.ListingType.Apartment,
                            regularUsers.Count > 1 ? regularUsers[1].Id : regularUsers[0].Id,
                            new List<ListingEnums.CommunalService> { ListingEnums.CommunalService.Included },
                            ListingEnums.OwnershipType.Without,
                            ListingEnums.NeighbourType.With,
                            DateTime.UtcNow.AddDays(-1)
                        ),

                        Listing.New(
                            ListingId.New(),
                            "Бюджетне місце на підселення (для хлопця)",
                            "Шукаємо одного хлопця на підселення в 2-кімнатну квартиру. Район 'Академмістечко', вул. Татарська. Проживання з двома іншими студентами. Оплата + комунальні.",
                            "вул. Татарська, 7, Острог",
                            50.3311,
                            26.5179,
                            3000.0f,
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