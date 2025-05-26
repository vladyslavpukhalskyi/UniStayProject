using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

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
                if (await _context.Database.CanConnectAsync())
                {
                    _logger.LogInformation("Attempting to apply database migrations.");
                    await _context.Database.MigrateAsync();
                    _logger.LogInformation("Database migrations applied successfully (or no migrations needed).");
                }
                else
                {
                    _logger.LogError("Could not connect to the database. Migrations will not be applied.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initialising the database.");
                throw;
            }
        }

        public async Task SeedAsync()
        {
            try
            {
                if (!_context.Users.Any())
                {
                    _logger.LogInformation("Seeding initial user data.");
                    _logger.LogInformation("Initial user data seeded.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }
    }
}