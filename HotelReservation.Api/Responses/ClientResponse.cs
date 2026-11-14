
namespace HotelReservation.Api.Responses
{
    // Завдання 2 і 6. DTO відповіді з даними клієнта
    public class ClientResponse
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public DateOnly RegistrationDate { get; set; }
    }
}
