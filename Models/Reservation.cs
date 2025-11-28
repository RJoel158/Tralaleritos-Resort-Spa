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

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        [DataType(DataType.Date)]
        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        [Display(Name = "Update Date")]
        public DateTime? UpdateDate { get; set; }

        //[Required]
        //public int UserId { get; set; }

        [Required(ErrorMessage = "Client is required")]
        [Display(Name = "Client")]
        public User? Client { get; set; }

        public List<ReservationDetail> Details { get; set; } = new List<ReservationDetail>();

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
