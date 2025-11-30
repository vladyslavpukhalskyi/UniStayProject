using System.Reflection;
using Application.Listings.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common 
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