using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResortTralaleritos.Data;
using ResortTralaleritos.Models;

namespace ResortTralaleritos.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly AppDbContext _context;

        public ReservationsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Reservations
                .Include(r => r.Client)
                .Include(r => r.ReservationRooms)
                    .ThenInclude(rr => rr.Room);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Reservations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Client)
                .Include(r => r.ReservationRooms)
                    .ThenInclude(rr => rr.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Reservations/Create
        public IActionResult Create()
        {
            ViewData["ClientId"] = new SelectList(_context.Users, "Id", "Email");
            // Cargar solo habitaciones disponibles
            ViewData["RoomId"] = new SelectList(
                _context.Rooms.Where(r => r.Status == RoomStatus.Available),
                "RoomId",
                "RoomNumber"
            );
            return View();
        }

        // POST: Reservations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Code,CheckInDate,CheckOutDate,Status,ClientId")] Reservation reservation, int RoomId)
        {
            if (ModelState.IsValid)
            {
                // Validar que la habitación existe y está disponible
                var room = await _context.Rooms.FindAsync(RoomId);
                if (room == null)
                {
                    ModelState.AddModelError("RoomId", "La habitación seleccionada no existe.");
                    ViewData["ClientId"] = new SelectList(_context.Users, "Id", "Email", reservation.ClientId);
                    ViewData["RoomId"] = new SelectList(
                        _context.Rooms.Where(r => r.Status == RoomStatus.Available),
                        "RoomId",
                        "RoomNumber",
                        RoomId
                    );
                    return View(reservation);
                }

                if (room.Status != RoomStatus.Available)
                {
                    ModelState.AddModelError("RoomId", "La habitación seleccionada no está disponible.");
                    ViewData["ClientId"] = new SelectList(_context.Users, "Id", "Email", reservation.ClientId);
                    ViewData["RoomId"] = new SelectList(
                        _context.Rooms.Where(r => r.Status == RoomStatus.Available),
                        "RoomId",
                        "RoomNumber",
                        RoomId
                    );
                    return View(reservation);
                }

                // Usar transacción para garantizar consistencia
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Guardar la reservación
                    _context.Add(reservation);
                    await _context.SaveChangesAsync();

                    // Crear el registro en ReservationRooms
                    var reservationRoom = new ReservationRoom
                    {
                        ReservationId = reservation.Id,
                        RoomId = RoomId
                    };
                    _context.ReservationRooms.Add(reservationRoom);

                    // Cambiar el estado de la habitación a Occupied
                    room.Status = RoomStatus.Occupied;
                    room.UpdateDate = DateTime.Now;
                    _context.Update(room);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            ViewData["ClientId"] = new SelectList(_context.Users, "Id", "Email", reservation.ClientId);
            ViewData["RoomId"] = new SelectList(
                _context.Rooms.Where(r => r.Status == RoomStatus.Available),
                "RoomId",
                "RoomNumber",
                RoomId
            );
            return View(reservation);
        }

        // GET: Reservations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            ViewData["ClientId"] = new SelectList(_context.Users, "Id", "Email", reservation.ClientId);
            return View(reservation);
        }

        // POST: Reservations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Code,CheckInDate,CheckOutDate,Status,UpdateDate,ClientId")] Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Obtener el registro original desde la base de datos
                    var existingReservation = await _context.Reservations.FindAsync(id);

                    if (existingReservation == null)
                        return NotFound();

                    // Actualizar SOLO los campos que sí deben cambiar
                    existingReservation.Code = reservation.Code;
                    existingReservation.CheckInDate = reservation.CheckInDate;
                    existingReservation.CheckOutDate = reservation.CheckOutDate;
                    existingReservation.Status = reservation.Status;
                    existingReservation.UpdateDate = DateTime.Now;
                    existingReservation.ClientId = reservation.ClientId;

                    // NO actualizar RegistrationDate
                    // existingService.RegistrationDate permanece igual

                    _context.Update(existingReservation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClientId"] = new SelectList(_context.Users, "Id", "Email", reservation.ClientId);
            return View(reservation);
        }

        // GET: Reservations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Client)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }
    }
}
