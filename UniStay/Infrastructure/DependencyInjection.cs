// Файл: Infrastructure/DependencyInjection.cs
using Application.Common.Interfaces; // Залишимо, якщо інші інтерфейси з цього простору імен використовуються
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Auth; // <<<< Цей using достатній для IJwtGenerator та IPasswordHasher
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services; // <<<< Для JwtGenerator та PasswordHasher (або Security, залежить від вашої структури)
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
// using IPasswordHasher = Application.Common.Interfaces.IPasswordHasher; // <<<< ВИДАЛЕНО: Цей alias зазвичай не потрібен, якщо IPasswordHasher знаходиться в Application.Common.Interfaces.Auth

namespace Infrastructure
{
    public static class DependencyInjection
    {
        // Назвіть метод так, як ви його викликаєте в Program.cs (наприклад, AddInfrastructureServices)
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 1. Реєстрація ApplicationDbContextInitialiser
            services.AddScoped<ApplicationDbContextInitialiser>();

            // 2. Реєстрація IPasswordHasher та IJwtGenerator
            // Переконайтеся, що 'PasswordHasher' - це фактична назва вашого класу реалізації.
            // Якщо у вас клас називається 'PasswordHasherService', то використовуйте 'PasswordHasherService'.
            services.AddScoped<IPasswordHash, PasswordHasher>();
            services.AddScoped<IJwtGenerator, JwtGenerator>();

            // 3. Реєстрація Репозиторіїв та Query-інтерфейсів
            // Переконайтеся, що ваші репозиторії дійсно реалізують відповідні інтерфейси Queries
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<IUsersQueries, UsersRepository>();

            services.AddScoped<IListingsRepository, ListingsRepository>();
            services.AddScoped<IListingsQueries, ListingsRepository>();

            services.AddScoped<IAmenitiesRepository, AmenitiesRepository>();
            services.AddScoped<IAmenitiesQueries, AmenitiesRepository>();

            services.AddScoped<IReviewsRepository, ReviewsRepository>();
            services.AddScoped<IReviewsQueries, ReviewsRepository>();

            services.AddScoped<IMessagesRepository, MessagesRepository>();
            services.AddScoped<IMessagesQueries, MessagesRepository>();

            services.AddScoped<IFavoritesRepository, FavoritesRepository>();
            services.AddScoped<IFavoritesQueries, FavoritesRepository>();

            services.AddScoped<IListingImagesRepository, ListingImagesRepository>();
            services.AddScoped<IListingImagesQueries, ListingImagesRepository>();


            // Додайте сюди інші сервіси, специфічні для інфраструктурного шару.

            return services;
        }
    }
}