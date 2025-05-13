using Microsoft.Extensions.DependencyInjection;
using System.Reflection; // Для Assembly.GetExecutingAssembly()
using FluentValidation; // Для AddValidatorsFromAssembly
// using AutoMapper; // Розкоментуйте, якщо використовуєте AutoMapper

namespace Application // Або ваш кореневий простір імен для цього проєкту
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // 1. Реєстрація MediatR
            // Сканує поточну збірку (Application) на наявність обробників команд, запитів, сповіщень
            // і реєструє їх.
            services.AddMediatR(cfg => 
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // 2. Реєстрація валідаторів FluentValidation
            // Сканує поточну збірку (Application) на наявність класів,
            // що успадковують AbstractValidator<T>, і реєструє їх.
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // 3. Реєстрація AutoMapper (якщо використовується)
            // Сканує поточну збірку на наявність профілів AutoMapper.
            // services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Тут можна додавати реєстрацію інших сервісів, специфічних для шару Application,
            // наприклад, кастомні сервіси бізнес-логіки, якщо вони не реалізуються через MediatR.

            return services;
        }
    }
}