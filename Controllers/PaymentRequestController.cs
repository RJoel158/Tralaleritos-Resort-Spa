using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
                .Include(r => r.Client)
                .Include(r => r.ReservationRooms)
                    .ThenInclude(rr => rr.Room)
                        .ThenInclude(room => room.RoomType)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
                return NotFound("Reserva no encontrada.");

            int nights = (reservation.CheckOutDate - reservation.CheckInDate).Days;
            if (nights <= 0) nights = 1;

            // Construcción del ViewModel
            var vm = new PaymentRequestViewModel
            {
                RequestCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                ReservationId = reservationId,
                GuestFullName = $"{reservation.Client.Name} {reservation.Client.LastName} {reservation.Client.SecondLastName}".Trim(),
                GuestEmail = reservation.Client.Email,
                CheckIn = reservation.CheckInDate,
                CheckOut = reservation.CheckOutDate,
                RoomDetails = reservation.ReservationRooms.Select(rr => new PaymentRequestRoomDetail
                {
                    RoomNumber = rr.Room.RoomNumber,
                    RoomType = rr.Room.RoomType.Name,
                    PricePerNight = rr.Room.RoomType.BasePrice,
                    Nights = nights,
                    Subtotal = rr.Room.RoomType.BasePrice * nights
                }).ToList()
            };

            vm.TotalAmount = vm.RoomDetails.Sum(r => r.Subtotal);

            // Guardar en la base de datos
            var paymentRequest = new PaymentRequest
            {
                ReservationId = reservationId,
                TotalAmount = vm.TotalAmount,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending",
                CreatedByUserId = User.Identity.Name // si quieres registrar quién creó la solicitud
            };

            foreach (var room in vm.RoomDetails)
            {
                paymentRequest.Items.Add(new PaymentRequestItem
                {
                    Description = $"Habitación {room.RoomNumber} ({room.RoomType})",
                    Amount = room.Subtotal
                });
            }

            _context.PaymentRequests.Add(paymentRequest);
            await _context.SaveChangesAsync();

            return View(vm);
        }

    }
}
