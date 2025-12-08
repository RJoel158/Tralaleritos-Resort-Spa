using ResortTralaleritos.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResortTralaleritos.Models
{
    public class PaymentRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReservationId { get; set; }

        [ForeignKey(nameof(ReservationId))]
        public virtual Reservation Reservation { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? CreatedByUserId { get; set; }

        public virtual ICollection<PaymentRequestItem> Items { get; set; } = new List<PaymentRequestItem>();
    }

    public class PaymentRequestItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PaymentRequestId { get; set; }

        [ForeignKey(nameof(PaymentRequestId))]
        public virtual PaymentRequest PaymentRequest { get; set; }

        [Required]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
    }
}
