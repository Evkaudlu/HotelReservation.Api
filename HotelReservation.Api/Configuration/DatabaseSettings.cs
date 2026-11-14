using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Api.Configuration
{
    public class DatabaseSettings
    {
        public const string SectionName = "DatabaseSettings";

        [Required(ErrorMessage = "Назва бази даних обов'язкова")]
        [MinLength(4, ErrorMessage =
            "Назва бази даних повинна містити мінімум 4 символи")]
        public string DatabaseName { get; set; } = string.Empty;
    }
}
