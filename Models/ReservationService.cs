using ResortTralaleritos.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class ReservationService
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ReservationId { get; set; }

    [ForeignKey(nameof(ReservationId))]
    public virtual Reservation Reservation { get; set; }

    [Required]
    public int ServiceId { get; set; }

    [ForeignKey(nameof(ServiceId))]
    public virtual Service Service { get; set; }

    [Required]
    [Range(1, 100)]
    public int Quantity { get; set; } = 1;
}
