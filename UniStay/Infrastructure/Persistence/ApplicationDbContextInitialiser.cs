using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Для логування
using System; // Для ArgumentNullException
using System.Threading.Tasks; // Для Task

namespace Infrastructure.Persistence
{
    public class ApplicationDbContextInitialiser
    {
        private readonly ILogger<ApplicationDbContextInitialiser> _logger;
        private readonly ApplicationDbContext _context;

        public ApplicationDbContextInitialiser(
            ILogger<ApplicationDbContextInitialiser> logger,
            ApplicationDbContext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Перевіряємо, чи можна підключитися до БД і чи є міграції
                if (await _context.Database.CanConnectAsync()) // Перевірка з'єднання
                {
                    // Застосовуємо всі очікуючі міграції
                    // Це створить БД, якщо її немає, і застосує схему
                    _logger.LogInformation("Attempting to apply database migrations.");
                    await _context.Database.MigrateAsync();
                    _logger.LogInformation("Database migrations applied successfully (or no migrations needed).");
                }
                else
                {
                    _logger.LogError("Could not connect to the database. Migrations will not be applied.");
                    // Можливо, тут варто викинути виняток або обробити ситуацію інакше
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initialising the database.");
                throw; // Перекидаємо виняток, щоб проблема була видимою при запуску
            }
        }

        public async Task SeedAsync()
        {
            try
            {
                // Спробуйте створити базу даних, якщо вона ще не існує
                // await _context.Database.EnsureCreatedAsync(); // Обережно з цим, якщо використовуються міграції.
                                                              // MigrateAsync зазвичай краще.

                // Тут буде ваша логіка заповнення початковими даними
                // Наприклад, створення користувача-адміністратора, стандартних ролей, зручностей тощо.
                if (!_context.Users.Any()) // Приклад: додати адміна, якщо користувачів немає
                {
                    _logger.LogInformation("Seeding initial user data.");
                    // _context.Users.Add(new Domain.Users.User(...)); // Додайте вашого адміна
                    // await _context.SaveChangesAsync();
                    _logger.LogInformation("Initial user data seeded.");
                }
                // Додайте іншу логіку seed тут
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }
    }
}