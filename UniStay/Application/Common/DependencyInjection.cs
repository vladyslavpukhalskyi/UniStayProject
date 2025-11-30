using Microsoft.Extensions.DependencyInjection; 
using System.Reflection; 
using FluentValidation;
using Application.Listings.Services;

namespace Application 
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg => 
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddScoped<IListingComparisonService, ListingComparisonService>();

            return services;
        }
    }
}