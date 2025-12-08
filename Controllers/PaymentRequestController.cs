using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResortTralaleritos.Data;
using ResortTralaleritos.Models;
using ResortTralaleritos.Models.ViewModels;

namespace ResortTralaleritos.Controllers
{
    public class PaymentRequestController : Controller
    {
        private readonly AppDbContext _context;

        public PaymentRequestController(AppDbContext context)
        {
            _context = context;
        }

        // GENERAR SOLICITUD DE PAGO
        public async Task<IActionResult> Generate(int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.ReservationRooms)
                    .ThenInclude(rr => rr.Room)
                        .ThenInclude(room => room.RoomType)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation == null)
                return NotFound("Reserva no encontrada.");

            // Cálculo de noches
            int nights = (reservation.CheckOut - reservation.CheckIn).Days;
            if (nights <= 0) nights = 1;

            // Construcción del ViewModel
            var vm = new PaymentRequestViewModel
            {
                RequestCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                ReservationId = reservationId,
                GuestFullName = reservation.User.FullName,
                GuestEmail = reservation.User.Email,
                CheckIn = reservation.CheckIn,
                CheckOut = reservation.CheckOut,
                RoomDetails = reservation.ReservationRooms.Select(rr => new PaymentRequestRoomDetail
                {
                    RoomNumber = rr.Room.Number,
                    RoomType = rr.Room.RoomType.Name,
                    PricePerNight = rr.Room.RoomType.Price,
                    Nights = nights,
                    Subtotal = rr.Room.RoomType.Price * nights
                }).ToList()
            };

            vm.TotalAmount = vm.RoomDetails.Sum(r => r.Subtotal);

            return View(vm);
        }
    }
}
