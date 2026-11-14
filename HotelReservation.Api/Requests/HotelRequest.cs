using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Api.Requests
{
    public class HotelRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        public DateOnly? OpeningDate { get; set; }
    }
}
