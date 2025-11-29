using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResortTralaleritos.Models
{
    public class ReservationDetail
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Check-In date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Check-In Date")]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Check-Out date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Check-Out Date")]
        public DateTime CheckOutDate { get; set; }

        [Required(ErrorMessage = "Number of guests is required")]
        [Range(1, 20)]
        public int NumberOfGuests { get; set; }

        [Display(Name = "Total")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        [Required(ErrorMessage = "Room is required")]
        [Display(Name = "Room")]
        public int RoomId { get; set; }

        public Room? Room { get; set; }

        [Required(ErrorMessage = "Reservation is required")]
        [Display(Name = "Reservation")]
        public int ReservationId { get; set; }

        public Reservation? Reservation { get; set; }
    }
}
