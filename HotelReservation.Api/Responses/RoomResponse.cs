
namespace HotelReservation.Api.Responses
{
    // Завдання 2 і 6. DTO відповіді з даними номера
    public class RoomResponse
    {
        public int Id { get; set; }

        public int HotelId { get; set; }

        public string Number { get; set; } = string.Empty;

        public string RoomType { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public decimal PricePerNight { get; set; }

        public bool IsAvailable { get; set; }
    }
}
