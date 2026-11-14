namespace HotelReservation.Api.Responses
{
    // Завдання 2 і 6. DTO відповіді з даними готелю
    public class HotelResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public int Rating { get; set; }

        public DateOnly OpeningDate { get; set; }
    }
}
