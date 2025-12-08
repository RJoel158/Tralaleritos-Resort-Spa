using System;
using System.Collections.Generic;

namespace ResortTralaleritos.Models.ViewModels
{
    public class PaymentRequestViewModel
    {
        public string RequestCode { get; set; }
        public int ReservationId { get; set; }
        public string GuestFullName { get; set; }
        public string GuestEmail { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public List<PaymentRequestRoomDetail> RoomDetails { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class PaymentRequestRoomDetail
    {
        public string RoomNumber { get; set; }
        public string RoomType { get; set; }
        public decimal PricePerNight { get; set; }
        public int Nights { get; set; }
        public decimal Subtotal { get; set; }
    }
}
