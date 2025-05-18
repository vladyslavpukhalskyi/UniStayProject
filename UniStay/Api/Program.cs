using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Api.Modules; // Для DbModule та SetupModule
using Application; // Для AddApplicationServices
using Microsoft.Extensions.Logging; // Для ILogger
// using System.Reflection; // Може знадобитися для AssemblyMarker, якщо Application/Infrastructure в інших збірках

var builder = WebApplication.CreateBuilder(args);

// --- Реєстрація сервісів ---

// 1. Сервіси API (Swagger/OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApplication();
builder.Services.AddSwaggerGen(options =>
{
    // Тут можна додати конфігурацію Swagger, наприклад, для JWT
    // options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { /* ... */ });
    // options.AddSecurityRequirement(new OpenApiSecurityRequirement { /* ... */ });
});

// 2. Налаштування контексту бази даних
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"),
        npgsqlOptionsAction: sqlOptions =>
        {
            // sqlOptions.EnableRetryOnFailure( // Налаштування політики повторних спроб
            //     maxRetryCount: 5,
            //     maxRetryDelay: TimeSpan.FromSeconds(30),
            //     errorCodesToAdd: null);
            sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        })
        .UseSnakeCaseNamingConvention());

// 3. Реєстрація ApplicationDbContextInitialiser (потрібен для DbModule)
// Ваш AddInfrastructure може це робити, але явна реєстрація тут теж ОК.
builder.Services.AddScoped<ApplicationDbContextInitialiser>();

// 4. Реєстрація сервісів з Infrastructure шару
// Цей метод має реєструвати репозиторії (IUsersRepository -> UsersRepository, etc.)
// та інші сервіси інфраструктури (наприклад, IPasswordHasher).
builder.Services.AddInfrastructureServices(builder.Configuration);

// 5. Реєстрація сервісів з Application шару
// Цей метод (з Application/DependencyInjection.cs) реєструє MediatR
// та валідатори FluentValidation з Application збірки.
builder.Services.AddApplicationServices();

// 6. Налаштування специфічних для API сервісів (включаючи інтеграцію FluentValidation)
// Цей метод з Api.Modules.SetupModule має викликати services.AddFluentValidationAutoValidation();
builder.Services.SetupApplicationServices(); // <--- ВАЖЛИВО: Переконайтесь, що цей виклик є

// 7. Додавання контролерів
builder.Services.AddControllers();
// ErrorHandler'и є статичними методами розширення, тому їх не потрібно реєструвати в DI.

// 8. Додавання логування
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders(); // Очистити провайдери за замовчуванням, якщо потрібно
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
    // Додайте інші провайдери логування за потреби (Serilog, NLog тощо)
});

// --- Створення додатку ---
var app = builder.Build();

// --- Ініціалізація бази даних та заповнення початковими даними ---
// Викликаємо метод з DbModule. Переконайтесь, що він також викликає SeedAsync всередині, якщо потрібно.
await app.InitialiseDbAsync();

// --- Налаштування конвеєра обробки HTTP-запитів ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        // c.RoutePrefix = string.Empty; // Для доступу до Swagger UI за кореневим URL
    });
    // app.UseDeveloperExceptionPage(); // Зазвичай не потрібен з .NET 6+, якщо є глобальний обробник
}
else
{
    // TODO: Налаштувати глобальний обробник винятків для Production
    // app.UseExceptionHandler("/Error"); // Або кастомний middleware
    app.UseHsts();
}

// app.UseHttpsRedirection(); // Рекомендовано для Production

// TODO: Налаштувати Authentication та Authorization
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

app.Run();