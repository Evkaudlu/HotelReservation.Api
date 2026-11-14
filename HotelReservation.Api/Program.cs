using HotelReservation.Api.Configuration;
using HotelReservation.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using HotelReservation.Api.Services.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Завдання 6. Реєстрація контролерів API
builder.Services.AddControllers();
// Завдання 7. Реєстрація глобального обробника помилок
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Завдання 4. Конфігурація та її валідація
builder.Services
    .AddOptions<DatabaseSettings>()
    .Bind(builder.Configuration.GetSection(
        DatabaseSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Завдання 5. Підключення бази даних InMemory
builder.Services.AddDbContext<HotelDbContext>((provider, options) =>
{
    var settings = provider
        .GetRequiredService<IOptions<DatabaseSettings>>()
        .Value;

    options.UseInMemoryDatabase(settings.DatabaseName);
});

var app = builder.Build();



// Завдання 4. Повідомлення про помилки конфігурації
try
{
    var databaseSettings = app.Services
     .GetRequiredService<IOptions<DatabaseSettings>>()
     .Value;

    // Завдання 5. Автоматичне заповнення бази даних
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider
            .GetRequiredService<HotelDbContext>();

        await DatabaseSeeder.SeedAsync(
            context,
            app.Environment);
    }
    // Завдання 7. Обробка винятків HTTP-запитів
    app.UseExceptionHandler();
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.MapControllers();

    app.Run();
}
catch (OptionsValidationException ex)
{
    Console.WriteLine(
        $"Помилка: Некоректна назва бази даних. {ex.Message}");
}
