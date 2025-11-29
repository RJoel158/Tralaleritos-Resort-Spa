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
    public class ReservationRoomsController : Controller
    {
        private readonly AppDbContext _context;

        public ReservationRoomsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ReservationRooms
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.ReservationRooms.Include(r => r.Reservation).Include(r => r.Room);
            return View(await appDbContext.ToListAsync());
        }

        // GET: ReservationRooms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservationRoom = await _context.ReservationRooms
                .Include(r => r.Reservation)
                .Include(r => r.Room)
                .FirstOrDefaultAsync(m => m.ReservationId == id);
            if (reservationRoom == null)
            {
                return NotFound();
            }

            return View(reservationRoom);
        }

        // GET: ReservationRooms/Create
        public IActionResult Create()
        {
            ViewData["ReservationId"] = new SelectList(_context.Reservations, "Id", "Code");
            ViewData["RoomId"] = new SelectList(_context.Rooms, "RoomId", "RoomNumber");
            return View();
        }

        // POST: ReservationRooms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReservationId,RoomId")] ReservationRoom reservationRoom)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reservationRoom);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReservationId"] = new SelectList(_context.Reservations, "Id", "Code", reservationRoom.ReservationId);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "RoomId", "RoomNumber", reservationRoom.RoomId);
            return View(reservationRoom);
        }

        // GET: ReservationRooms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservationRoom = await _context.ReservationRooms.FindAsync(id);
            if (reservationRoom == null)
            {
                return NotFound();
            }
            ViewData["ReservationId"] = new SelectList(_context.Reservations, "Id", "Code", reservationRoom.ReservationId);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "RoomId", "RoomNumber", reservationRoom.RoomId);
            return View(reservationRoom);
        }

        // POST: ReservationRooms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReservationId,RoomId")] ReservationRoom reservationRoom)
        {
            if (id != reservationRoom.ReservationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservationRoom);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationRoomExists(reservationRoom.ReservationId))
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
            ViewData["ReservationId"] = new SelectList(_context.Reservations, "Id", "Code", reservationRoom.ReservationId);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "RoomId", "RoomNumber", reservationRoom.RoomId);
            return View(reservationRoom);
        }

        // GET: ReservationRooms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservationRoom = await _context.ReservationRooms
                .Include(r => r.Reservation)
                .Include(r => r.Room)
                .FirstOrDefaultAsync(m => m.ReservationId == id);
            if (reservationRoom == null)
            {
                return NotFound();
            }

            return View(reservationRoom);
        }

        // POST: ReservationRooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservationRoom = await _context.ReservationRooms.FindAsync(id);
            if (reservationRoom != null)
            {
                _context.ReservationRooms.Remove(reservationRoom);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationRoomExists(int id)
        {
            return _context.ReservationRooms.Any(e => e.ReservationId == id);
        }
    }
}
