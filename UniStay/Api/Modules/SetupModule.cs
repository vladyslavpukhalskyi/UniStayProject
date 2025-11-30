using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Api.Dtos.Listing;
using Api.Services;
using Application.Common.Interfaces.Services;

namespace Api.Modules
{
    public static class SetupModule
    {
        public static void SetupApplicationServices(this IServiceCollection services)
        {
            services.AddValidators();
            services.AddScoped<IListingComparisonDtoMapper, ListingComparisonDtoMapper>();
            
            // SignalR Chat Notification Service
            services.AddScoped<IChatNotificationService, ChatNotificationService>();
        }

        private static void AddValidators(this IServiceCollection services)
        {
            services.AddFluentValidationAutoValidation()
                .AddValidatorsFromAssemblyContaining<Program>();
        }
    }
}