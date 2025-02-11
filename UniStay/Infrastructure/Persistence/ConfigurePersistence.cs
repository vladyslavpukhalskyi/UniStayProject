using Application.Common.Interfaces.Repositories;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence
{
    public static class ConfigurePersistence
    {
        public static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            // Налаштування контексту бази даних
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("Default"))
                    .UseSnakeCaseNamingConvention());

            // Ініціалізація бази даних
            services.AddScoped<ApplicationDbContextInitialiser>();

            // Додавання репозиторіїв
            services.AddRepositories();
        }

        private static void AddRepositories(this IServiceCollection services)
        {
            // Додавання репозиторіїв для кожної сутності
            services.AddScoped<IAmenitiesRepository, AmenitiesRepository>();
            services.AddScoped<IFavoritesRepository, FavoritesRepository>();
            services.AddScoped<IListingImagesRepository, ListingImagesRepository>();
            services.AddScoped<IListingsRepository, ListingsRepository>();
            services.AddScoped<IMessagesRepository, MessagesRepository>();
            services.AddScoped<IReviewsRepository, ReviewsRepository>();
            services.AddScoped<IUsersRepository, UsersRepository>();
        }
    }
}