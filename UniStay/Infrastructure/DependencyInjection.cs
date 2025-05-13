// Файл: Infrastructure/DependencyInjection.cs
using Application.Common.Interfaces;
using Application.Common.Interfaces.Queries; // Для IPasswordHasher та інтерфейсів репозиторіїв
using Application.Common.Interfaces.Repositories; // Для інтерфейсів репозиторіїв
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Security; // Для PasswordHasherService
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        // Назвіть метод так, як ви його викликаєте в Program.cs (наприклад, AddInfrastructure)
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // 1. Реєстрація ApplicationDbContextInitialiser
            // (ApplicationDbContext реєструється напряму в Program.cs у вашому випадку,
            // але його теж можна перенести сюди для кращої організації)
            // Якщо ApplicationDbContext вже зареєстрований в Program.cs, тут його повторно реєструвати не треба.
            // services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseNpgsql(configuration.GetConnectionString("Default"),
            //        o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
            //    .UseSnakeCaseNamingConvention());

            services.AddScoped<ApplicationDbContextInitialiser>();

            // 2. Реєстрація IPasswordHasher
            services.AddScoped<IPasswordHasher, PasswordHasherService>();

            // 3. Реєстрація Репозиторіїв (та Query-інтерфейсів, якщо вони реалізовані репозиторіями)
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<IUsersQueries, UsersRepository>(); // Якщо UsersRepository реалізує IUsersQueries

            services.AddScoped<IListingsRepository, ListingsRepository>();
            services.AddScoped<IListingsQueries, ListingsRepository>(); // Якщо ListingsRepository реалізує IListingsQueries

            services.AddScoped<IAmenitiesRepository, AmenitiesRepository>();
            services.AddScoped<IAmenitiesQueries, AmenitiesRepository>(); // Якщо AmenitiesRepository реалізує IAmenitiesQueries
            
            services.AddScoped<IReviewsRepository, ReviewsRepository>();
            services.AddScoped<IReviewsQueries, ReviewsRepository>(); // Якщо ReviewsRepository реалізує IReviewsQueries

            services.AddScoped<IMessagesRepository, MessagesRepository>();
            services.AddScoped<IMessagesQueries, MessagesRepository>(); // Якщо MessagesRepository реалізує IMessagesQueries

            services.AddScoped<IFavoritesRepository, FavoritesRepository>();
            services.AddScoped<IFavoritesQueries, FavoritesRepository>(); // Якщо FavoritesRepository реалізує IFavoritesQueries

            services.AddScoped<IListingImagesRepository, ListingImagesRepository>();
            services.AddScoped<IListingImagesQueries, ListingImagesRepository>(); // Якщо ListingImagesRepository реалізує IListingImagesQueries


            // Додайте сюди інші сервіси, специфічні для інфраструктурного шару.

            return services;
        }
    }
}