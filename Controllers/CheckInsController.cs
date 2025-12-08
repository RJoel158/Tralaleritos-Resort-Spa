using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResortTralaleritos.Data;
using ResortTralaleritos.Models;
using ResortTralaleritos.Services;

namespace ResortTralaleritos.Controllers
{
    public class CheckInsController : Controller
    {
        private readonly ICheckInService _checkInService;
        private readonly AppDbContext _context;

        public CheckInsController(ICheckInService checkInService, AppDbContext context)
        {
            _checkInService = checkInService;
            _context = context;
        }

        // GET: CheckIns
        public async Task<IActionResult> Index(string searchTerm, DateTime? fromDate, DateTime? toDate, 
            string status, int pageNumber = 1)
        {
            var filter = new CheckInSearchDto
            {
                SearchTerm = searchTerm,
                FromDate = fromDate,
                ToDate = toDate,
                Status = status,
                PageNumber = pageNumber,
                PageSize = 10,
                SortBy = "CheckInDate",
                SortDescending = true
            };

            var (checkIns, totalCount) = await _checkInService.SearchCheckInsAsync(filter);

            ViewBag.TotalCount = totalCount;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = 10;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / 10.0);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.Status = status;

            return View(checkIns);
        }

        // GET: CheckIns/New
        public async Task<IActionResult> New()
        {
            // Obtener habitaciones disponibles (Status = Available)
            var filter = new RoomFilterDto { Status = RoomStatus.Available, PageSize = 100 };
            var (availableRooms, _) = await _checkInService.SearchAvailableRoomsAsync(filter);

            ViewBag.AvailableRooms = availableRooms;

            var checkInModel = new { Guest = new Guest(), CheckIn = new CheckIn() };
            return View(checkInModel);
        }

        // POST: CheckIns/New
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(Guest guest, CheckIn checkIn)
        {
            try
            {
                // Validar datos del huésped
                if (!ModelState.IsValid)
                {
                    var filter = new RoomFilterDto { Status = RoomStatus.Available, PageSize = 100 };
                    var (availableRooms, _) = await _checkInService.SearchAvailableRoomsAsync(filter);
                    ViewBag.AvailableRooms = availableRooms;
                    return View(new { Guest = guest, CheckIn = checkIn });
                }

                // Obtener o crear huésped
                var existingGuest = await _checkInService.GetOrCreateGuestAsync(guest);
                checkIn.GuestId = existingGuest.GuestId;

                // Registrar check-in
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var registeredBy = User.Identity?.Name ?? "System";

                var registeredCheckIn = await _checkInService.RegisterCheckInAsync(checkIn, registeredBy, ipAddress);

                TempData["SuccessMessage"] = $"Check-in registrado exitosamente para {existingGuest.FullName} en la habitación {registeredCheckIn.Room.RoomNumber}";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var filter = new RoomFilterDto { Status = RoomStatus.Available, PageSize = 100 };
                var (availableRooms, _) = await _checkInService.SearchAvailableRoomsAsync(filter);
                ViewBag.AvailableRooms = availableRooms;
                return View(new { Guest = guest, CheckIn = checkIn });
            }
        }

        // GET: CheckIns/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checkIn = await _checkInService.GetCheckInByIdAsync(id.Value);
            if (checkIn == null)
            {
                return NotFound();
            }

            return View(checkIn);
        }

        // GET: CheckIns/SearchRooms (AJAX)
        [HttpGet]
        public async Task<IActionResult> SearchRooms(string term)
        {
            var filter = new RoomFilterDto 
            { 
                RoomType = term,
                Status = RoomStatus.Available,
                PageSize = 20 
            };

            var (rooms, _) = await _checkInService.SearchAvailableRoomsAsync(filter);

            var roomsList = rooms.Select(r => new 
            { 
                id = r.RoomId, 
                text = $"Habitación {r.RoomNumber} - {r.RoomType?.Name} (${r.PricePerNight})" 
            }).ToList();

            return Json(roomsList);
        }
    }
}
