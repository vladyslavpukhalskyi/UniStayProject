using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Додаємо інфраструктуру
builder.Services.AddInfrastructure(builder.Configuration);

// Налаштування контексту бази даних
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
        .UseSnakeCaseNamingConvention());

// Створення додатку
var app = builder.Build();

// Ініціалізація бази даних
using (var scope = app.Services.CreateScope())
{
    var dbContextInitializer = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
    await dbContextInitializer.InitializeAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();