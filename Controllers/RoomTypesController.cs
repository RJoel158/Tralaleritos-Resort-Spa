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
    public class RoomTypesController : Controller
    {
        private readonly AppDbContext _context;

        public RoomTypesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: RoomTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.RoomTypes.ToListAsync());
        }

        // GET: RoomTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomType = await _context.RoomTypes
                .FirstOrDefaultAsync(m => m.RoomTypeId == id);
            if (roomType == null)
            {
                return NotFound();
            }

            return View(roomType);
        }

        // GET: RoomTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RoomTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomTypeId,Name,Description,BasePrice,DefaultCapacity,DefaultBeds,RegistrationDate")] RoomType roomType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(roomType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(roomType);
        }

        // GET: RoomTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomType = await _context.RoomTypes.FindAsync(id);
            if (roomType == null)
            {
                return NotFound();
            }
            return View(roomType);
        }

        // POST: RoomTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoomTypeId,Name,Description,BasePrice,DefaultCapacity,DefaultBeds,UpdateDate")] RoomType roomType)
        {
            if (id != roomType.RoomTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Obtener el registro original desde la base de datos
                    var existingRoomType = await _context.RoomTypes.FindAsync(id);

                    if (existingRoomType == null)
                        return NotFound();

                    // Actualizar SOLO los campos que sí deben cambiar
                    existingRoomType.Name = roomType.Name;
                    existingRoomType.Description = roomType.Description;
                    existingRoomType.BasePrice = roomType.BasePrice;
                    existingRoomType.DefaultCapacity = roomType.DefaultCapacity;
                    existingRoomType.DefaultBeds = roomType.DefaultBeds;
                    existingRoomType.UpdateDate = DateTime.Now;

                    // NO actualizar RegistrationDate
                    // existingService.RegistrationDate permanece igual

                    _context.Update(existingRoomType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomTypeExists(roomType.RoomTypeId))
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
            return View(roomType);
        }

        // GET: RoomTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomType = await _context.RoomTypes
                .FirstOrDefaultAsync(m => m.RoomTypeId == id);
            if (roomType == null)
            {
                return NotFound();
            }

            return View(roomType);
        }

        // POST: RoomTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var roomType = await _context.RoomTypes.FindAsync(id);
            if (roomType != null)
            {
                _context.RoomTypes.Remove(roomType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomTypeExists(int id)
        {
            return _context.RoomTypes.Any(e => e.RoomTypeId == id);
        }
    }
}
