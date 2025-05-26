using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Api.Modules;
using Application;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- Реєстрація сервісів ---

// 1. Сервіси API (Swagger/OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApplication(); // Assuming this is a custom extension method for Application layer services
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "UniStay API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ДОДАНО: Конфігурація JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)), // Use '!' to assert non-null
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"]!, // Use '!' to assert non-null
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"]!, // Use '!' to assert non-null
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // Токен вважається дійсним до закінчення терміну дії
        };
    });

builder.Services.AddAuthorization(); // Обов'язково додаємо сервіси авторизації

// 2. Налаштування контексту бази даних
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"),
        npgsqlOptionsAction: sqlOptions =>
        {
            sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        })
        .UseSnakeCaseNamingConvention());

// 3. Реєстрація ApplicationDbContextInitialiser
builder.Services.AddScoped<ApplicationDbContextInitialiser>();

// 4. Реєстрація сервісів з Infrastructure шару
builder.Services.AddInfrastructureServices(builder.Configuration);

// 5. Реєстрація сервісів з Application шару
builder.Services.AddApplicationServices(); // Assuming this is a custom extension method for Application layer services

// 6. Налаштування специфічних для API сервісів (включаючи інтеграцію FluentValidation)
builder.Services.SetupApplicationServices(); // Assuming this is a custom extension method for API specific services

// 7. Додавання контролерів
builder.Services.AddControllers();

// 8. Додавання логування
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

// --- Створення додатку ---
var app = builder.Build();

// --- Ініціалізація бази даних та заповнення початковими даними ---
await app.InitialiseDbAsync();

// --- Налаштування конвеєра обробки HTTP-запитів ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UniStay API V1");
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// ВАЖЛИВО: Увімкніть Authentication та Authorization
// Це потрібно для роботи з JWT-токенами та атрибутами [Authorize]
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();