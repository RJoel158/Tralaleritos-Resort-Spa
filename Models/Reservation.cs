using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResortTralaleritos.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Code is required")]
        [Display(Name = "Code")]
        [StringLength(10, ErrorMessage = "{0} must be minimum {2} and maximum {1} characters", MinimumLength = 3)]
        public string? Code { get; set; }

        [Required(ErrorMessage = "Check-In date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Check-In Date")]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Check-Out date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Check-Out Date")]
        public DateTime CheckOutDate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        [DataType(DataType.Date)]
        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        [Display(Name = "Update Date")]
        public DateTime? UpdateDate { get; set; }

        [Required(ErrorMessage = "Client is required")]
        [Display(Name = "Client")]
        public int ClientId { get; set; }

        public User? Client { get; set; }

        // Relación many-to-many explícita
        public ICollection<ReservationRoom> ReservationRooms { get; set; } = new List<ReservationRoom>();

        //public ICollection<Room> Rooms { get; set; } = new List<Room>();

        //public int? BookingDiscountId { get; set; }
        //public BookingDiscount? BookingDiscount { get; set; }
    }

    public enum ReservationStatus
    {
        Active,
        Pending,
        Disabled
    }
}
