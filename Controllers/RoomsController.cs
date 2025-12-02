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
            // Redirigir al método de búsqueda con filtros por defecto
            return RedirectToAction(nameof(Search));
        }

        /// <summary>
        /// GET: Rooms/Search
        /// Implementa CU-HABITACION-03: Permite buscar habitaciones con filtros avanzados
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

            // Preparar data para la vista
            var roomTypes = await _context.RoomTypes.Select(rt => rt.Name).Distinct().ToListAsync();
            ViewBag.RoomTypes = new SelectList(roomTypes, selectedValue: roomType);
            ViewBag.Statuses = Enum.GetValues(typeof(RoomStatus))
                .Cast<RoomStatus>()
                .ToList();

            // Guardar filtros actuales en ViewBag para mantener en la vista
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
            return View();
        }

        // POST: Rooms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomId,RoomNumber,RoomType,Description,Capacity,Beds,PricePerNight,IsAvailable,Status,CreatedAt,UpdatedAt")] Room room)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = User?.Identity?.Name ?? "Sistema";
                    var createdRoom = await _roomService.CreateRoomAsync(room, currentUser);
                    TempData["SuccessMessage"] = "Habitación creada exitosamente";
                    return RedirectToAction(nameof(Search));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
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
            return View(room);
        }

        // POST: Rooms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoomId,RoomNumber,RoomType,Description,Capacity,Beds,PricePerNight,IsAvailable,Status,CreatedAt,UpdatedAt")] Room room)
        {
            if (id != room.RoomId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Obtener el usuario actual (podría mejorarse con autenticación)
                    var currentUser = User?.Identity?.Name ?? "Sistema";
                    
                    // Usar el servicio para actualizar y registrar auditoría
                    var updatedRoom = await _roomService.UpdateRoomAsync(id, room, currentUser);
                    
                    TempData["SuccessMessage"] = "Habitación actualizada exitosamente";
                    return RedirectToAction(nameof(Search));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);
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
            }
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
        /// GET: Rooms/AuditHistory/5
        /// Muestra el historial de auditoría de una habitación
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
