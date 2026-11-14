
using HotelReservation.Api.Data;
using HotelReservation.Api.Models;
using HotelReservation.Api.Requests;
using HotelReservation.Api.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Api.Controllers
{
    // Завдання 6. REST API для бронювання номерів
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly HotelDbContext context;

        public BookingsController(HotelDbContext context)
        {
            this.context = context;
        }

        // Завдання 6. Отримання всіх бронювань
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<BookingResponse>>> GetAll()
        {
            var bookings = await context.Bookings.ToListAsync();
            return Ok(bookings.Select(ToResponse).ToList());
        }

        // Завдання 6. Пошук бронювання за ID
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookingResponse>> GetById(int id)
        {
            if (id <= 0)
                return BadRequest("Некоректний ID бронювання");

            var booking = await context.Bookings.FindAsync(id);

            if (booking == null)
                return NotFound("Бронювання не знайдено");

            return Ok(ToResponse(booking));
        }

        // Завдання 6. Створення бронювання
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BookingResponse>> Create(
            [FromBody] BookingRequest request)
        {
            var error = await ValidateBooking(
                request, null);

            if (error != null)
                return BadRequest(error);

            var room = await context.Rooms.FindAsync(request.RoomId);

            var checkIn = request.CheckInDate!.Value;
            var checkOut = request.CheckOutDate!.Value;

            int nights = checkOut.DayNumber - checkIn.DayNumber;

            var booking = new Booking
            {
                RoomId = request.RoomId,
                ClientId = request.ClientId,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                TotalPrice = nights * room!.PricePerNight,
                Status = "Pending",
                BookingDate = DateOnly.FromDateTime(DateTime.Today)
            };

            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = booking.Id },
                ToResponse(booking));
        }

        // Завдання 6. Оновлення бронювання
        [HttpPut("{id:int}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] BookingRequest request)
        {
            if (id <= 0)
                return BadRequest("Некоректний ID");

            var booking = await context.Bookings.FindAsync(id);

            if (booking == null)
                return NotFound();

            if (booking.Status == "Cancelled")
                return BadRequest(
                    "Скасоване бронювання не можна редагувати");

            var error = await ValidateBooking(request, id);

            if (error != null)
                return BadRequest(error);

            var room = await context.Rooms.FindAsync(request.RoomId);

            var checkIn = request.CheckInDate!.Value;
            var checkOut = request.CheckOutDate!.Value;

            int nights = checkOut.DayNumber - checkIn.DayNumber;

            booking.RoomId = request.RoomId;
            booking.ClientId = request.ClientId;
            booking.CheckInDate = checkIn;
            booking.CheckOutDate = checkOut;
            booking.TotalPrice = nights * room!.PricePerNight;

            await context.SaveChangesAsync();

            return NoContent();
        }

        // Завдання 6. Скасування бронювання
        [HttpPatch("{id:int}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Cancel(int id)
        {
            if (id <= 0)
                return BadRequest("Некоректний ID");

            var booking = await context.Bookings.FindAsync(id);

            if (booking == null)
                return NotFound();

            if (booking.Status == "Cancelled")
                return BadRequest("Бронювання вже скасовано");

            booking.Status = "Cancelled";
            await context.SaveChangesAsync();

            return Ok(ToResponse(booking));
        }


        // Завдання 6 і 7. Підтвердження бронювання
        // з перевіркою доступності номера
        [HttpPatch("{id:int}/confirm")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Confirm(int id)
        {
            if (id <= 0)
                return BadRequest("Некоректний ID");

            var booking = await context.Bookings.FindAsync(id);

            if (booking == null)
                return NotFound("Бронювання не знайдено");

            if (booking.Status != "Pending")
                return BadRequest(
                    "Підтвердити можна лише бронювання Pending");

            var room = await context.Rooms.FindAsync(booking.RoomId);

            if (room == null || !room.IsAvailable)
                return BadRequest("Номер недоступний");

            // Перевірка перетину дат з іншими бронюваннями
            bool occupied = await context.Bookings.AnyAsync(b =>
                b.RoomId == booking.RoomId &&
                b.Id != booking.Id &&
                b.Status != "Cancelled" &&
                booking.CheckInDate < b.CheckOutDate &&
                booking.CheckOutDate > b.CheckInDate);

            if (occupied)
                return BadRequest(
                    "Номер уже заброньований на ці дати");

            booking.Status = "Confirmed";

            await context.SaveChangesAsync();

            return Ok(ToResponse(booking));
        }

   

        // Завдання 6. Видалення бронювання
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IResult> Delete(int id)
        {
            if (id <= 0)
                return Results.BadRequest("Некоректний ID");

            var booking = await context.Bookings.FindAsync(id);

            if (booking == null)
                return Results.NotFound();

            context.Bookings.Remove(booking);
            await context.SaveChangesAsync();

            return Results.NoContent();
        }

        // Завдання 6. Бронювання конкретного клієнта
        [HttpGet("by-client/{clientId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<BookingResponse>>> GetByClient(
            int clientId)
        {
            if (clientId <= 0)
                return BadRequest("Некоректний ID клієнта");

            if (!await context.Clients.AnyAsync(
                c => c.Id == clientId))
                return NotFound("Клієнта не знайдено");

            var bookings = await context.Bookings
                .Where(b => b.ClientId == clientId)
                .ToListAsync();

            return Ok(bookings.Select(ToResponse).ToList());
        }

        // Завдання 6. Перевірка даних бронювання
        private async Task<string?> ValidateBooking(
            BookingRequest request, int? excludedId)
        {
            if (!request.CheckInDate.HasValue ||
                !request.CheckOutDate.HasValue)
                return "Потрібно вказати обидві дати";

            var checkIn = request.CheckInDate.Value;
            var checkOut = request.CheckOutDate.Value;

            if (checkIn < DateOnly.FromDateTime(DateTime.Today))
                return "Дата заселення не може бути в минулому";

            if (checkOut <= checkIn)
                return "Дата виселення має бути пізнішою";

            var room = await context.Rooms.FindAsync(request.RoomId);

            if (room == null)
                return "Готельний номер не знайдено";

            if (!room.IsAvailable)
                return "Номер недоступний для бронювання";

            if (!await context.Clients.AnyAsync(
                c => c.Id == request.ClientId))
                return "Клієнта не знайдено";

            // Перевірка перетину дат бронювань
            bool occupied = await context.Bookings.AnyAsync(b =>
                b.RoomId == request.RoomId &&
                b.Id != excludedId &&
                b.Status != "Cancelled" &&
                checkIn < b.CheckOutDate &&
                checkOut > b.CheckInDate);

            if (occupied)
                return "Номер уже заброньований на ці дати";

            return null;
        }

        // Перетворення моделі на DTO відповіді
        private static BookingResponse ToResponse(Booking booking)
        {
            return new BookingResponse
            {
                Id = booking.Id,
                RoomId = booking.RoomId,
                ClientId = booking.ClientId,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                BookingDate = booking.BookingDate
            };
        }
    }
}
