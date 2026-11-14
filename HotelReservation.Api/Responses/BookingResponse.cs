
namespace HotelReservation.Api.Responses
{
    // Завдання 2 і 6. DTO відповіді про бронювання
    public class BookingResponse
    {
        public int Id { get; set; }

        public int RoomId { get; set; }

        public int ClientId { get; set; }

        public DateOnly CheckInDate { get; set; }

        public DateOnly CheckOutDate { get; set; }

        public decimal TotalPrice { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateOnly BookingDate { get; set; }
    }
}
