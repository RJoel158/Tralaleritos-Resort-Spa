using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResortTralaleritos.Data;
using ResortTralaleritos.Models;
using ResortTralaleritos.Services;

namespace ResortTralaleritos.Controllers
{
    public class RoomsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IRoomService _roomService;

        public RoomsController(AppDbContext context, IRoomService roomService)
        {
            _context = context;
            _roomService = roomService;
        }

        // GET: Rooms
        public async Task<IActionResult> Index()
        {
            return RedirectToAction(nameof(Search));
        }

        /// <summary>
        /// GET: Rooms/Search - Advanced room search with filters
        /// </summary>
        public async Task<IActionResult> Search(string? roomNumber, string? roomType, RoomStatus? status,
            decimal? minPrice, decimal? maxPrice, int? minCapacity, bool? isAvailable,
            int pageNumber = 1, string sortBy = "RoomNumber", bool sortDescending = false)
        {
            var filter = new RoomFilterDto
            {
                RoomNumber = roomNumber,
                RoomType = roomType,
                Status = status,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                MinCapacity = minCapacity,
                IsAvailable = isAvailable,
                PageNumber = pageNumber,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var (rooms, totalCount) = await _roomService.SearchRoomsAsync(filter);

            // Prepare data for view
            var roomTypes = await _context.RoomTypes.Select(rt => rt.Name).Distinct().ToListAsync();
            ViewBag.RoomTypes = new SelectList(roomTypes, selectedValue: roomType);
            ViewBag.Statuses = Enum.GetValues(typeof(RoomStatus))
                .Cast<RoomStatus>()
                .ToList();

            // Save current filters in ViewBag
            ViewBag.CurrentFilter = filter;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);
            ViewBag.CurrentPage = pageNumber;

            return View(rooms);
        }

        // GET: Rooms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(m => m.RoomId == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // GET: Rooms/Create
        public IActionResult Create()
        {
            ViewData["RoomTypeId"] = new SelectList(_context.RoomTypes, "RoomTypeId", "Name");
            return View();
        }

        // POST: Rooms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomId,RoomNumber,Description,PricePerNight,Status,RoomTypeId")] Room room)
        {
            if (ModelState.IsValid)
            {
                _context.Add(room);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoomTypeId"] = new SelectList(_context.RoomTypes, "RoomTypeId", "Name", room.RoomTypeId);
            return View(room);
        }

        // GET: Rooms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            ViewData["RoomTypeId"] = new SelectList(_context.RoomTypes, "RoomTypeId", "Name", room.RoomTypeId);
            return View(room);
        }

        // POST: Rooms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoomId,RoomNumber,Description,PricePerNight,Status,UpdateDate,RoomTypeId")] Room room)
        {
            if (id != room.RoomId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Obtener el registro original desde la base de datos
                    var existingRoom = await _context.Rooms.FindAsync(id);

                    if (existingRoom == null)
                        return NotFound();

                    // Actualizar SOLO los campos que sí deben cambiar
                    existingRoom.RoomNumber = room.RoomNumber;
                    existingRoom.Description = room.Description;
                    existingRoom.PricePerNight = room.PricePerNight;
                    existingRoom.Status = room.Status;
                    existingRoom.UpdateDate = DateTime.Now;
                    existingRoom.RoomTypeId = room.RoomTypeId;

                    // NO actualizar RegistrationDate
                    // existingService.RegistrationDate permanece igual

                    _context.Update(existingRoom);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.RoomId))
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
            ViewData["RoomTypeId"] = new SelectList(_context.RoomTypes, "RoomTypeId", "Name", room.RoomTypeId);
            return View(room);
        }

        // GET: Rooms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(m => m.RoomId == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // POST: Rooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.RoomId == id);
        }

        /// <summary>
        /// GET: Rooms/AuditHistory/5 - Show audit history for a room
        /// </summary>
        public async Task<IActionResult> AuditHistory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _roomService.GetRoomByIdAsync(id.Value);
            if (room == null)
            {
                return NotFound();
            }

            var auditLogs = await _roomService.GetRoomAuditHistoryAsync(id.Value);

            ViewBag.Room = room;
            return View(auditLogs);
        }
    }
}
