using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Modules
{
    public static class SetupModule
    {
        public static void SetupApplicationServices(this IServiceCollection services)
        {
            services.AddValidators();
        }

        private static void AddValidators(this IServiceCollection services)
        {
            services.AddFluentValidationAutoValidation()
                .AddValidatorsFromAssemblyContaining<Program>();
        }
    }
}