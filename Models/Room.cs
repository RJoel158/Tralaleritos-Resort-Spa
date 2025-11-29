using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResortTralaleritos.Models
{
    public class Room
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Room number is required")]
        [Display(Name = "Room Number")]
        [StringLength(10)]
        public string? RoomNumber { get; set; }

        [Display(Name = "Description")]
        [StringLength(300)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price per night is required")]
        [Display(Name = "Price Per Night")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PricePerNight { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public RoomStatus Status { get; set; } = RoomStatus.Available;

        [DataType(DataType.Date)]
        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        
        [DataType(DataType.Date)]
        [Display(Name = "Update Date")]
        public DateTime? UpdateDate { get; set; }

        [Required(ErrorMessage = "Room type is required")]
        [Display(Name = "Room Type")]
        public int RoomTypeId { get; set; }

        public RoomType? RoomType { get; set; }

        public ICollection<ReservationRoom> ReservationRooms { get; set; } = new List<ReservationRoom>();

        //public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }

    public enum RoomStatus
    {
        Available,
        Occupied,
        Cleaning,
        Maintenance,
        Reserved
    }
}
