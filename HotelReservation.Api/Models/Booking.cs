using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Api.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue)]
        public int RoomId { get; set; }

        [Range(1, int.MaxValue)]
        public int ClientId { get; set; }

        [Required]
        public DateOnly CheckInDate { get; set; }

        [Required]
        public DateOnly CheckOutDate { get; set; }

        [Range(1, 1000000)]
        public decimal TotalPrice { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        public DateOnly BookingDate { get; set; }
    }
}
