using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Api.Models
{
    public class Hotel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Вкажіть назву готелю")]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть місто")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть адресу")]
        public string Address { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Рейтинг має бути від 1 до 5")]
        public int Rating { get; set; }

        public DateOnly OpeningDate { get; set; }
    }
}
