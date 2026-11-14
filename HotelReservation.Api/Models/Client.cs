using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Api.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Вкажіть ім'я клієнта")]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть прізвище клієнта")]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть електронну пошту")]
        [EmailAddress(ErrorMessage = "Некоректна електронна пошта")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть номер телефону")]
        [Phone(ErrorMessage = "Некоректний номер телефону")]
        public string PhoneNumber { get; set; } = string.Empty;

        public DateOnly RegistrationDate { get; set; }
    }
}
