
using HotelReservation.Api.Data;
using HotelReservation.Api.Models;
using HotelReservation.Api.Requests;
using HotelReservation.Api.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Api.Controllers
{
    // Завдання 6. REST API для управління готелями
    [ApiController]
    [Route("api/[controller]")]
    public class HotelsController : ControllerBase
    {
        private readonly HotelDbContext context;
        private readonly ILogger<HotelsController> logger;

        public HotelsController(
            HotelDbContext context,
            ILogger<HotelsController> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        // Завдання 6. GET: отримання всіх готелів
        // Тип повернення: Specific Type
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<List<HotelResponse>> GetAll()
        {
            return await context.Hotels
                .Select(h => new HotelResponse
                {
                    Id = h.Id,
                    Name = h.Name,
                    City = h.City,
                    Address = h.Address,
                    Rating = h.Rating,
                    OpeningDate = h.OpeningDate
                })
                .ToListAsync();
        }

        // Завдання 6. GET: отримання готелю за ID
        // Тип повернення: IActionResult
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest("Некоректний ID готелю");

            var hotel = await context.Hotels.FindAsync(id);

            if (hotel == null)
                return NotFound("Готель не знайдено");

            return Ok(ToResponse(hotel));
        }

        // Завдання 6. POST: створення готелю
        // Тип повернення: ActionResult<T>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<HotelResponse>> Create(
            [FromBody] HotelRequest request)
        {
            if (request.OpeningDate == null)
                return BadRequest("Вкажіть дату відкриття");

            var hotel = new Hotel
            {
                Name = request.Name,
                City = request.City,
                Address = request.Address,
                Rating = request.Rating,
                OpeningDate = request.OpeningDate.Value
            };

            context.Hotels.Add(hotel);
            await context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = hotel.Id },
                ToResponse(hotel));
        }

        // Завдання 6. PUT: оновлення готелю
        // Тип повернення: ActionResult<T>
        [HttpPut("{id:int}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HotelResponse>> Update(
            int id,
            [FromBody] HotelRequest request)
        {
            if (id <= 0)
                return BadRequest("Некоректний ID");

            var hotel = await context.Hotels.FindAsync(id);

            if (hotel == null)
                return NotFound();

            if (request.OpeningDate == null)
                return BadRequest("Вкажіть дату відкриття");

            hotel.Name = request.Name;
            hotel.City = request.City;
            hotel.Address = request.Address;
            hotel.Rating = request.Rating;
            hotel.OpeningDate = request.OpeningDate.Value;

            await context.SaveChangesAsync();

            return NoContent();
        }

        // Завдання 6. DELETE: видалення готелю
        // Тип повернення: HttpResults
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IResult> Delete(int id)
        {
            if (id <= 0)
                return Results.BadRequest("Некоректний ID");

            var hotel = await context.Hotels.FindAsync(id);

            if (hotel == null)
                return Results.NotFound();

            // Не дозволяємо видаляти готель із номерами
            if (await context.Rooms.AnyAsync(r => r.HotelId == id))
                return Results.BadRequest(
                    "Спочатку потрібно видалити номери готелю");

            context.Hotels.Remove(hotel);
            await context.SaveChangesAsync();

            return Results.NoContent();
        }

        // Завдання 6. GET: пошук готелів за списком ID
        // Тип повернення: IActionResult
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Search(
            [FromQuery] string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return BadRequest("Список ID порожній");

            var values = ids.Split(',');

            var hotelIds = new List<int>();

            foreach (var value in values)
            {
                if (!int.TryParse(value.Trim(), out int id)
                    || id <= 0)
                {
                    return BadRequest(
                        "Список містить некоректний ID");
                }

                hotelIds.Add(id);
            }

            var hotels = await context.Hotels
                .Where(h => hotelIds.Contains(h.Id))
                .ToListAsync();

            return Ok(hotels.Select(ToResponse).ToList());
        }

        // Завдання 6. POST: масове створення готелів
        [HttpPost("bulk")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<HotelResponse>>> BulkCreate(
            [FromBody] List<HotelRequest> requests)
        {
            if (requests == null || requests.Count == 0)
                return BadRequest("Список готелів порожній");

            foreach (var request in requests)
            {
                if (!TryValidateModel(request)
                    || request.OpeningDate == null)
                {
                    return BadRequest(
                        "Список містить некоректні дані");
                }
            }

            var hotels = requests.Select(r => new Hotel
            {
                Name = r.Name,
                City = r.City,
                Address = r.Address,
                Rating = r.Rating,
                OpeningDate = r.OpeningDate!.Value
            }).ToList();

            await context.Hotels.AddRangeAsync(hotels);
            await context.SaveChangesAsync();

            return StatusCode(
                StatusCodes.Status201Created,
                hotels.Select(ToResponse).ToList());
        }

        // Завдання 6. GET: фільтрація за містом
        [HttpGet("by-city")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<HotelResponse>>> GetByCity(
            [FromQuery] string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return BadRequest("Вкажіть місто");

            var hotels = await context.Hotels
                .Where(h => h.City.ToLower() == city.ToLower())
                .ToListAsync();

            return Ok(hotels.Select(ToResponse).ToList());
        }

        // Допоміжний метод перетворення моделі на DTO
        private static HotelResponse ToResponse(Hotel hotel)
        {
            return new HotelResponse
            {
                Id = hotel.Id,
                Name = hotel.Name,
                City = hotel.City,
                Address = hotel.Address,
                Rating = hotel.Rating,
                OpeningDate = hotel.OpeningDate
            };
        }
    }
}
