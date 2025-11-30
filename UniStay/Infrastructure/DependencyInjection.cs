using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Auth;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<ApplicationDbContextInitialiser>();

            services.AddScoped<IPasswordHash, PasswordHasher>();
            services.AddScoped<IJwtGenerator, JwtGenerator>();

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

            services.AddScoped<IChatsRepository, ChatsRepository>();
            services.AddScoped<IChatsQueries, ChatsRepository>();

            services.AddScoped<IChatMembersRepository, ChatMembersRepository>();
            services.AddScoped<IChatMessagesRepository, ChatMessagesRepository>();

            return services;
        }
    }
}