
using System.Text.Json;
using HotelReservation.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Api.Data
{
    // Завдання 5. Початкове заповнення бази даних
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(
            HotelDbContext context,
            IWebHostEnvironment environment)
        {
            // Шлях до папки з JSON-файлами
            string dataPath = Path.Combine(
                environment.ContentRootPath,
                "src",
                "data");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Завантаження готелів
            if (!await context.Hotels.AnyAsync())
            {
                var hotels = await LoadAsync<Hotel>(
                    dataPath, "hotels.json", options);

                await context.Hotels.AddRangeAsync(hotels);
            }

            // Завантаження номерів
            if (!await context.Rooms.AnyAsync())
            {
                var rooms = await LoadAsync<Room>(
                    dataPath, "rooms.json", options);

                await context.Rooms.AddRangeAsync(rooms);
            }

            // Завантаження клієнтів
            if (!await context.Clients.AnyAsync())
            {
                var clients = await LoadAsync<Client>(
                    dataPath, "clients.json", options);

                await context.Clients.AddRangeAsync(clients);
            }

            // Завантаження бронювань
            if (!await context.Bookings.AnyAsync())
            {
                var bookings = await LoadAsync<Booking>(
                    dataPath, "bookings.json", options);

                await context.Bookings.AddRangeAsync(bookings);
            }

            // Збереження змін
            await context.SaveChangesAsync();

            Console.WriteLine(
                "Тестові дані успішно завантажено.");

            Console.WriteLine(
                $"Готелів: {await context.Hotels.CountAsync()}");

            Console.WriteLine(
                $"Номерів: {await context.Rooms.CountAsync()}");

            Console.WriteLine(
                $"Клієнтів: {await context.Clients.CountAsync()}");

            Console.WriteLine(
                $"Бронювань: {await context.Bookings.CountAsync()}");
        }

        // Метод читання JSON-файлів
        private static async Task<List<T>> LoadAsync<T>(
            string dataPath,
            string fileName,
            JsonSerializerOptions options)
        {
            string filePath = Path.Combine(
                dataPath, fileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    $"Не знайдено файл: {fileName}",
                    filePath);
            }

            string json = await File.ReadAllTextAsync(
                filePath);

            return JsonSerializer.Deserialize<List<T>>(
                json, options)
                ?? throw new InvalidOperationException(
                    $"Не вдалося прочитати {fileName}");
        }
    }
}
