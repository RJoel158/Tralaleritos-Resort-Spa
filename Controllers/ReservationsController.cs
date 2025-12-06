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
            // No cargar habitaciones inicialmente, se cargarán dinámicamente según fechas
            return View();
        }

        // API: Obtener habitaciones disponibles según rango de fechas
        [HttpGet]
        public async Task<JsonResult> GetAvailableRooms(DateTime checkInDate, DateTime checkOutDate)
        {
            if (checkInDate >= checkOutDate)
            {
                return Json(new { success = false, message = "Fechas inválidas" });
            }

            // Obtener IDs de habitaciones que tienen reservas activas o pendientes que se solapan con las fechas
            var occupiedRoomIds = await _context.ReservationRooms
                .Include(rr => rr.Reservation)
                .Where(rr => (rr.Reservation.Status == ReservationStatus.Active || 
                              rr.Reservation.Status == ReservationStatus.Pending) &&
                             rr.Reservation.CheckInDate < checkOutDate &&
                             checkInDate < rr.Reservation.CheckOutDate)
                .Select(rr => rr.RoomId)
                .Distinct()
                .ToListAsync();

            // Obtener TODAS las habitaciones que NO tengan conflictos de fechas
            // (independientemente de su estado actual, porque podrían estar ocupadas HOY pero libres en fechas futuras)
            var availableRooms = await _context.Rooms
                .Where(r => !occupiedRoomIds.Contains(r.RoomId))
                .Select(r => new { r.RoomId, r.RoomNumber })
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();

            return Json(new { success = true, rooms = availableRooms });
        }

        // POST: Reservations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Code,CheckInDate,CheckOutDate,ClientId")] Reservation reservation, int RoomId)
        {
            if (ModelState.IsValid)
            {
                // Validar fechas
                if (reservation.CheckInDate >= reservation.CheckOutDate)
                {
                    ModelState.AddModelError("CheckOutDate", "La fecha de Check-Out debe ser posterior a la fecha de Check-In.");
                    ViewData["ClientId"] = new SelectList(_context.Users, "Id", "Email", reservation.ClientId);
                    return View(reservation);
                }

                // Validar que la habitación existe
                var room = await _context.Rooms.FindAsync(RoomId);
                if (room == null)
                {
                    ModelState.AddModelError("RoomId", "La habitación seleccionada no existe.");
                    ViewData["ClientId"] = new SelectList(_context.Users, "Id", "Email", reservation.ClientId);
                    return View(reservation);
                }

                // Validar solapamiento de fechas con otras reservas activas o pendientes
                var hasConflict = await _context.ReservationRooms
                    .Include(rr => rr.Reservation)
                    .Where(rr => rr.RoomId == RoomId &&
                                 (rr.Reservation.Status == ReservationStatus.Active || 
                                  rr.Reservation.Status == ReservationStatus.Pending) &&
                                 rr.Reservation.CheckInDate < reservation.CheckOutDate &&
                                 reservation.CheckInDate < rr.Reservation.CheckOutDate)
                    .AnyAsync();

                if (hasConflict)
                {
                    ModelState.AddModelError("CheckInDate", 
                        $"La habitación {room.RoomNumber} ya tiene una reserva activa o pendiente en el rango de fechas seleccionado. Por favor, elija otras fechas o una habitación diferente.");
                    ViewData["ClientId"] = new SelectList(_context.Users, "Id", "Email", reservation.ClientId);
                    return View(reservation);
                }

                // Usar transacción para garantizar consistencia
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Establecer valores por defecto
                    reservation.RegistrationDate = DateTime.Now;
                    reservation.Status = ReservationStatus.Pending;

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

                    // Cambiar el estado de la habitación SOLO si es la próxima reserva para esa habitación
                    // Si la habitación está Available, la ponemos Reserved
                    // Si está Occupied o Reserved por otra reserva actual, la dejamos así (esta es una reserva futura)
                    if (room.Status == RoomStatus.Available)
                    {
                        room.Status = RoomStatus.Reserved;
                        room.UpdateDate = DateTime.Now;
                        _context.Update(room);
                    }

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
                    // Obtener el registro original desde la base de datos con habitaciones
                    var existingReservation = await _context.Reservations
                        .Include(r => r.ReservationRooms)
                            .ThenInclude(rr => rr.Room)
                        .FirstOrDefaultAsync(r => r.Id == id);

                    if (existingReservation == null)
                        return NotFound();

                    // Actualizar SOLO los campos que sí deben cambiar
                    existingReservation.Code = reservation.Code;
                    existingReservation.CheckInDate = reservation.CheckInDate;
                    existingReservation.CheckOutDate = reservation.CheckOutDate;
                    var previousStatus = existingReservation.Status;
                    existingReservation.Status = reservation.Status;
                    existingReservation.UpdateDate = DateTime.Now;
                    existingReservation.ClientId = reservation.ClientId;

                    // Actualizar estados de habitaciones según el nuevo estado de la reserva
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        foreach (var rr in existingReservation.ReservationRooms)
                        {
                            var room = rr.Room;
                            if (room == null) continue;

                            if (existingReservation.Status == ReservationStatus.Active)
                            {
                                room.Status = RoomStatus.Occupied;
                            }
                            else if (existingReservation.Status == ReservationStatus.Pending)
                            {
                                room.Status = RoomStatus.Reserved;
                            }
                            else if (existingReservation.Status == ReservationStatus.Disabled)
                            {
                                // Al desactivar, verificar si hay reservas futuras pendientes para esta habitación
                                var hasFutureReservation = await _context.ReservationRooms
                                    .Include(r => r.Reservation)
                                    .Where(r => r.RoomId == room.RoomId &&
                                               r.Reservation.Status == ReservationStatus.Pending &&
                                               r.Reservation.CheckInDate >= DateTime.Today)
                                    .OrderBy(r => r.Reservation.CheckInDate)
                                    .AnyAsync();

                                if (hasFutureReservation)
                                {
                                    // Si hay reserva futura pendiente, marcar como Reserved
                                    room.Status = RoomStatus.Reserved;
                                }
                                else
                                {
                                    // Si no hay reservas futuras, marcar como Available
                                    room.Status = RoomStatus.Available;
                                }
                            }

                            room.UpdateDate = DateTime.Now;
                            _context.Update(room);
                        }

                        _context.Update(existingReservation);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
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
            var reservation = await _context.Reservations
                .Include(r => r.ReservationRooms)
                    .ThenInclude(rr => rr.Room)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation != null)
            {
                // Usar transacción para garantizar consistencia
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Cambiar estado de las habitaciones a Available
                    foreach (var reservationRoom in reservation.ReservationRooms)
                    {
                        if (reservationRoom.Room != null)
                        {
                            reservationRoom.Room.Status = RoomStatus.Available;
                            reservationRoom.Room.UpdateDate = DateTime.Now;
                            _context.Update(reservationRoom.Room);
                        }
                    }

                    // Eliminar la reserva (las ReservationRooms se eliminan en cascada)
                    _context.Reservations.Remove(reservation);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }
    }
}
