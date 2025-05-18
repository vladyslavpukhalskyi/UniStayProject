using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Modules // Або інший простір імен, який ви використовуєте для модулів API
{
    public static class SetupModule
    {
        public static void SetupApplicationServices(this IServiceCollection services) // Перейменував для загальності
        {
            // Тут можна додавати реєстрацію інших сервісів рівня Application/Api пізніше
            services.AddValidators();
        }

        private static void AddValidators(this IServiceCollection services)
        {
            // Реєструє FluentValidation для автоматичної валідації моделей ASP.NET Core
            services.AddFluentValidationAutoValidation()
                .AddValidatorsFromAssemblyContaining<Program>();

            // Сканує збірку, в якій знаходиться клас Program (зазвичай це ваш API проєкт)
            // та реєструє всі валідатори, що успадковують AbstractValidator<T>

            // Якщо ваші валідатори в іншому проекті (наприклад, Application),
            // вам потрібно вказати клас-маркер з того проекту:
            // services.AddValidatorsFromAssemblyContaining<Application.Common.Interfaces.IApplicationDbContext>(); // Приклад
        }
    }
}