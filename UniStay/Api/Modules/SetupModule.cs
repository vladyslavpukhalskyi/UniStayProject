using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Api.Dtos.Listing;

namespace Api.Modules
{
    public static class SetupModule
    {
        public static void SetupApplicationServices(this IServiceCollection services)
        {
            services.AddValidators();
            services.AddScoped<IListingComparisonDtoMapper, ListingComparisonDtoMapper>();
        }

        private static void AddValidators(this IServiceCollection services)
        {
            services.AddFluentValidationAutoValidation()
                .AddValidatorsFromAssemblyContaining<Program>();
        }
    }
}