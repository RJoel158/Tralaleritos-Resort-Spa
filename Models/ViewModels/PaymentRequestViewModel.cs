using System;
using System.Collections.Generic;

namespace ProyectoHotel.Models.ViewModels
{
    public class PaymentRequestRoomDetail
    {
        public string RoomNumber { get; set; }
        public string RoomType { get; set; }
        public decimal PricePerNight { get; set; }
        public int Nights { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class PaymentRequestViewModel
    {
        public string RequestCode { get; set; }
        public int ReservationId { get; set; }

        // Datos del cliente
        public string GuestFullName { get; set; }
        public string GuestEmail { get; set; }

        // Datos de la estancia
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }

        // Detalles habitaciones
        public List<PaymentRequestRoomDetail> RoomDetails { get; set; }

        // Total
        public decimal TotalAmount { get; set; }
    }
}
