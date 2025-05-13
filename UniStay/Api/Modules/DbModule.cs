using Infrastructure.Persistence; // Переконайтеся, що цей using правильний для вашого ApplicationDbContextInitialiser
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks; // Додано для Task

namespace Api.Modules // Або інший простір імен, який ви використовуєте для модулів API
{
    public static class DbModule
    {
        public static async Task InitialiseDbAsync(this WebApplication app) // Додав Async до назви для ясності
        {
            using var scope = app.Services.CreateScope();
            var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
            await initialiser.InitializeAsync(); // Цей метод також має бути асинхронним в ApplicationDbContextInitialiser
            // Якщо у вас є окремий метод для seed, його можна викликати тут
            // await initialiser.SeedAsync();
        }
    }
}