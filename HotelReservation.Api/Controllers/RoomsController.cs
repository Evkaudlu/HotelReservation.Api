
using HotelReservation.Api.Data;
using HotelReservation.Api.Models;
using HotelReservation.Api.Requests;
using HotelReservation.Api.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Api.Controllers
{
    // Завдання 6. Управління готельними номерами
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly HotelDbContext context;

        public RoomsController(HotelDbContext context)
        {
            this.context = context;
        }

        // Завдання 6. Отримання всіх номерів
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<RoomResponse>>> GetAll()
        {
            var rooms = await context.Rooms.ToListAsync();

            return Ok(rooms.Select(ToResponse).ToList());
        }

        // Завдання 6. Отримання номера за ID
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RoomResponse>> GetById(int id)
        {
            if (id <= 0)
                return BadRequest("Некоректний ID номера");

            var room = await context.Rooms.FindAsync(id);

            if (room == null)
                return NotFound("Номер не знайдено");

            return Ok(ToResponse(room));
        }

        // Завдання 6. Створення нового номера
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RoomResponse>> Create(
            [FromBody] RoomRequest request)
        {
            if (!await context.Hotels.AnyAsync(
                    h => h.Id == request.HotelId))
            {
                return BadRequest("Готель не знайдено");
            }

            if (await context.Rooms.AnyAsync(r =>
                    r.HotelId == request.HotelId &&
                    r.Number == request.Number))
            {
                return BadRequest(
                    "Номер із таким позначенням уже існує в готелі");
            }

            var room = new Room
            {
                HotelId = request.HotelId,
                Number = request.Number,
                RoomType = request.RoomType,
                Capacity = request.Capacity,
                PricePerNight = request.PricePerNight,
                IsAvailable = request.IsAvailable
            };

            context.Rooms.Add(room);
            await context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = room.Id },
                ToResponse(room));
        }

        // Завдання 6. Оновлення інформації про номер
        [HttpPut("{id:int}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Update(
            int id,
            [FromBody] RoomRequest request)
        {
            if (id <= 0)
                return BadRequest("Некоректний ID номера");

            var room = await context.Rooms.FindAsync(id);

            if (room == null)
                return NotFound();

            if (!await context.Hotels.AnyAsync(
                    h => h.Id == request.HotelId))
            {
                return BadRequest("Готель не знайдено");
            }

            if (await context.Rooms.AnyAsync(r =>
                    r.Id != id &&
                    r.HotelId == request.HotelId &&
                    r.Number == request.Number))
            {
                return BadRequest("Такий номер уже існує");
            }

            room.HotelId = request.HotelId;
            room.Number = request.Number;
            room.RoomType = request.RoomType;
            room.Capacity = request.Capacity;
            room.PricePerNight = request.PricePerNight;
            room.IsAvailable = request.IsAvailable;

            await context.SaveChangesAsync();

            return NoContent();
        }

        // Завдання 6. Видалення номера
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IResult> Delete(int id)
        {
            if (id <= 0)
                return Results.BadRequest("Некоректний ID");

            var room = await context.Rooms.FindAsync(id);

            if (room == null)
                return Results.NotFound();

            if (await context.Bookings.AnyAsync(
                    b => b.RoomId == id))
            {
                return Results.BadRequest(
                    "Неможливо видалити номер із бронюваннями");
            }

            context.Rooms.Remove(room);
            await context.SaveChangesAsync();

            return Results.NoContent();
        }

        // Завдання 6. Пошук номерів за готелем
        [HttpGet("by-hotel/{hotelId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<RoomResponse>>> GetByHotel(
            int hotelId)
        {
            if (hotelId <= 0)
                return BadRequest("Некоректний ID готелю");

            if (!await context.Hotels.AnyAsync(
                    h => h.Id == hotelId))
            {
                return NotFound("Готель не знайдено");
            }

            var rooms = await context.Rooms
                .Where(r => r.HotelId == hotelId)
                .ToListAsync();

            return Ok(rooms.Select(ToResponse).ToList());
        }

        // Завдання 6. Пошук номерів за кількістю гостей
        [HttpGet("by-capacity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<RoomResponse>>> GetByCapacity(
            [FromQuery] int capacity)
        {
            if (capacity <= 0)
                return BadRequest("Кількість гостей має бути додатною");

            var rooms = await context.Rooms
                .Where(r => r.Capacity >= capacity)
                .ToListAsync();

            return Ok(rooms.Select(ToResponse).ToList());
        }

        // Перетворення моделі Room на DTO
        private static RoomResponse ToResponse(Room room)
        {
            return new RoomResponse
            {
                Id = room.Id,
                HotelId = room.HotelId,
                Number = room.Number,
                RoomType = room.RoomType,
                Capacity = room.Capacity,
                PricePerNight = room.PricePerNight,
                IsAvailable = room.IsAvailable
            };
        }
    }
}
