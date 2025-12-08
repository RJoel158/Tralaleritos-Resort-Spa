using ResortTralaleritos.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourProjectNamespace.Models
{
    public class PaymentRequest
    {
        [Key]
        public int Id { get; set; }

        // Link to reservation
        [Required]
        public int ReservationId { get; set; }

        [ForeignKey(nameof(ReservationId))]
        public virtual Reservation Reservation { get; set; }

        // Total at creation time
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // Could be Pending, Paid, Cancelled
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Who generated the request (user id/email)
        public string CreatedByUserId { get; set; }

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
