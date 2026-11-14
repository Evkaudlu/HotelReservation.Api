
using HotelReservation.Api.Data;
using HotelReservation.Api.Models;
using HotelReservation.Api.Requests;
using HotelReservation.Api.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Api.Controllers
{
    // Завдання 6. REST API для управління клієнтами
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly HotelDbContext context;

        public ClientsController(HotelDbContext context)
        {
            this.context = context;
        }

        // Завдання 6. Отримання всіх клієнтів
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ClientResponse>>> GetAll()
        {
            var clients = await context.Clients.ToListAsync();

            return Ok(clients.Select(ToResponse).ToList());
        }

        // Завдання 6. Пошук клієнта за ID
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClientResponse>> GetById(int id)
        {
            if (id <= 0)
                return BadRequest("Некоректний ID клієнта");

            var client = await context.Clients.FindAsync(id);

            if (client == null)
                return NotFound("Клієнта не знайдено");

            return Ok(ToResponse(client));
        }

        // Завдання 6. Створення клієнта
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ClientResponse>> Create(
            [FromBody] ClientRequest request)
        {
            if (await context.Clients.AnyAsync(
                c => c.Email == request.Email))
            {
                return BadRequest(
                    "Клієнт із такою поштою вже існує");
            }

            var client = new Client
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                RegistrationDate = DateOnly.FromDateTime(
                    DateTime.Today)
            };

            context.Clients.Add(client);
            await context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = client.Id },
                ToResponse(client));
        }

        // Завдання 6. Оновлення клієнта
        [HttpPut("{id:int}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] ClientRequest request)
        {
            if (id <= 0)
                return BadRequest("Некоректний ID");

            var client = await context.Clients.FindAsync(id);

            if (client == null)
                return NotFound();

            if (await context.Clients.AnyAsync(c =>
                c.Id != id && c.Email == request.Email))
            {
                return BadRequest(
                    "Така електронна пошта вже використовується");
            }

            client.FirstName = request.FirstName;
            client.LastName = request.LastName;
            client.Email = request.Email;
            client.PhoneNumber = request.PhoneNumber;

            await context.SaveChangesAsync();

            return NoContent();
        }

        // Завдання 6. Видалення клієнта
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IResult> Delete(int id)
        {
            if (id <= 0)
                return Results.BadRequest("Некоректний ID");

            var client = await context.Clients.FindAsync(id);

            if (client == null)
                return Results.NotFound();

            if (await context.Bookings.AnyAsync(
                b => b.ClientId == id))
            {
                return Results.BadRequest(
                    "Клієнта з бронюваннями видаляти не можна");
            }

            context.Clients.Remove(client);
            await context.SaveChangesAsync();

            return Results.NoContent();
        }

        // Завдання 6. Пошук клієнта за email
        [HttpGet("by-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClientResponse>> GetByEmail(
            [FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Вкажіть email");

            var client = await context.Clients
                .FirstOrDefaultAsync(c => c.Email == email);

            if (client == null)
                return NotFound("Клієнта не знайдено");

            return Ok(ToResponse(client));
        }

        // Перетворення моделі Client на DTO
        private static ClientResponse ToResponse(Client client)
        {
            return new ClientResponse
            {
                Id = client.Id,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,
                PhoneNumber = client.PhoneNumber,
                RegistrationDate = client.RegistrationDate
            };
        }
    }
}
