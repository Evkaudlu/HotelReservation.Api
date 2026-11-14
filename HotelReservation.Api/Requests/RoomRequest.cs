
using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Api.Requests
{
    // Завдання 2 і 6. DTO запиту для готельного номера
    public class RoomRequest
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "Вкажіть правильний ID готелю")]
        public int HotelId { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 1)]
        public string Number { get; set; } = string.Empty;

        [Required]
        public string RoomType { get; set; } = string.Empty;

        [Range(1, 10)]
        public int Capacity { get; set; }

        [Range(1, 100000)]
        public decimal PricePerNight { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}
