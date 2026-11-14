
using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Api.Requests
{
    // Завдання 2 і 6. DTO створення бронювання
    public class BookingRequest
    {
        [Range(1, int.MaxValue)]
        public int RoomId { get; set; }

        [Range(1, int.MaxValue)]
        public int ClientId { get; set; }

        [Required]
        public DateOnly? CheckInDate { get; set; }

        [Required]
        public DateOnly? CheckOutDate { get; set; }
    }
}
