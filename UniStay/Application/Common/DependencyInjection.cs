using Microsoft.Extensions.DependencyInjection; // Для IServiceCollection та методів розширення
using System.Reflection; // Для Assembly.GetExecutingAssembly()
using FluentValidation; // Для AddValidatorsFromAssembly
// using AutoMapper; // Розкоментуйте, якщо використовуєте AutoMapper
// using MediatR; // Зазвичай не потрібен напряму тут, якщо використовується AddMediatR з конфігурацією

namespace Application // Або ваш кореневий простір імен для цього проєкту
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // 1. Реєстрація MediatR
            // Сканує поточну збірку (Application) на наявність обробників команд, запитів, сповіщень
            // і реєструє їх.
            // Для цього методу потрібен NuGet пакет MediatR (версії 12+) або 
            // MediatR.Extensions.Microsoft.DependencyInjection (для старіших версій MediatR).
            services.AddMediatR(cfg => 
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // 2. Реєстрація валідаторів FluentValidation
            // Сканує поточну збірку (Application) на наявність класів,
            // що успадковують AbstractValidator<T>, і реєструє їх.
            // Для цього методу потрібен NuGet пакет FluentValidation.DependencyInjectionExtensions.
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // 3. Реєстрація AutoMapper (якщо використовується)
            // Сканує поточну збірку на наявність профілів AutoMapper.
            // Потрібен NuGet пакет AutoMapper.Extensions.Microsoft.DependencyInjection.
            // services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Тут можна додавати реєстрацію інших сервісів, специфічних для шару Application,
            // наприклад, кастомні сервіси бізнес-логіки (якщо вони не реалізуються виключно через MediatR),
            // обробники доменних подій (якщо ви їх використовуєте і реєструєте окремо) тощо.
            // Наприклад:
            // services.AddScoped<IMyApplicationService, MyApplicationService>();

            return services;
        }
    }
}