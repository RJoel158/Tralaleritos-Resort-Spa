using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ResortTralaleritos.Data;
using ResortTralaleritos.Models;

namespace ResortTralaleritos.Services
{
    public interface ICheckInService
    {
        Task<(List<CheckIn> checkIns, int totalCount)> SearchCheckInsAsync(CheckInSearchDto filter);
        Task<CheckIn?> GetCheckInByIdAsync(int checkInId);
        Task<Guest?> GetGuestByIdAsync(int guestId);
        Task<Guest> GetOrCreateGuestAsync(Guest guest);
        Task<CheckIn> RegisterCheckInAsync(CheckIn checkIn, string registeredBy, string ipAddress);
        Task<Room?> GetAvailableRoomAsync(int roomId);
        Task UpdateRoomStatusAsync(int roomId, int status);
        Task<(List<Room> rooms, int totalCount)> SearchAvailableRoomsAsync(RoomFilterDto filter);
    }

    public class CheckInService : ICheckInService
    {
        private readonly AppDbContext _context;

        public CheckInService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<CheckIn> checkIns, int totalCount)> SearchCheckInsAsync(CheckInSearchDto filter)
        {
            var query = _context.CheckIns
                .Include(c => c.Guest)
                .Include(c => c.Room)
                .AsQueryable();

            // Filtrar por término de búsqueda (nombre del huésped o identificación)
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(c => 
                    c.Guest.FirstName.Contains(filter.SearchTerm) ||
                    c.Guest.LastName.Contains(filter.SearchTerm) ||
                    c.Guest.IdentificationNumber.Contains(filter.SearchTerm)
                );
            }

            // Filtrar por rango de fechas
            if (filter.FromDate.HasValue)
            {
                query = query.Where(c => c.CheckInDate >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(c => c.CheckInDate <= filter.ToDate.Value);
            }

            // Filtrar por estado
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(c => c.Room.Status.ToString() == filter.Status);
            }

            int totalCount = await query.CountAsync();

            // Ordenar
            query = filter.SortDescending
                ? query.OrderByDescending(c => EF.Property<object>(c, filter.SortBy))
                : query.OrderBy(c => EF.Property<object>(c, filter.SortBy));

            // Paginar
            var checkIns = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (checkIns, totalCount);
        }

        public async Task<CheckIn?> GetCheckInByIdAsync(int checkInId)
        {
            return await _context.CheckIns
                .Include(c => c.Guest)
                .Include(c => c.Room)
                .FirstOrDefaultAsync(c => c.CheckInId == checkInId);
        }

        public async Task<Guest?> GetGuestByIdAsync(int guestId)
        {
            return await _context.Guests.FirstOrDefaultAsync(g => g.GuestId == guestId);
        }

        public async Task<Guest> GetOrCreateGuestAsync(Guest guest)
        {
            // Buscar si el huésped ya existe por identificación
            var existingGuest = await _context.Guests
                .FirstOrDefaultAsync(g => g.IdentificationNumber == guest.IdentificationNumber);

            if (existingGuest != null)
            {
                return existingGuest;
            }

            // Crear nuevo huésped
            _context.Guests.Add(guest);
            await _context.SaveChangesAsync();
            return guest;
        }

        public async Task<CheckIn> RegisterCheckInAsync(CheckIn checkIn, string registeredBy, string ipAddress)
        {
            try
            {
                // Validar que el huésped existe
                var guest = await GetGuestByIdAsync(checkIn.GuestId);
                if (guest == null)
                {
                    throw new InvalidOperationException("El huésped no existe");
                }

                // Validar que la habitación existe y está disponible
                var room = await GetAvailableRoomAsync(checkIn.RoomId);
                if (room == null)
                {
                    throw new InvalidOperationException("La habitación no está disponible");
                }

                // Configurar datos de auditoría
                checkIn.RegisteredBy = registeredBy;
                checkIn.IpAddress = ipAddress;
                checkIn.SystemRegisteredDate = DateTime.Now;

                // Agregar check-in
                _context.CheckIns.Add(checkIn);

                // Actualizar estado de la habitación a "Ocupada"
                room.Status = RoomStatus.Occupied;
                _context.Rooms.Update(room);

                await _context.SaveChangesAsync();
                return checkIn;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al registrar check-in: {ex.Message}", ex);
            }
        }

        public async Task<Room?> GetAvailableRoomAsync(int roomId)
        {
            return await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.RoomId == roomId && r.Status == RoomStatus.Available);
        }

        public async Task UpdateRoomStatusAsync(int roomId, int status)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room != null)
            {
                room.Status = (RoomStatus)status;
                _context.Rooms.Update(room);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(List<Room> rooms, int totalCount)> SearchAvailableRoomsAsync(RoomFilterDto filter)
        {
            var query = _context.Rooms
                .Include(r => r.RoomType)
                .Where(r => r.Status == RoomStatus.Available)
                .AsQueryable();

            // Filtrar por tipo de habitación
            if (!string.IsNullOrWhiteSpace(filter.RoomType))
            {
                query = query.Where(r => r.RoomType != null && r.RoomType.Name.Contains(filter.RoomType));
            }

            // Filtrar por precio
            if (filter.MinPrice.HasValue)
            {
                query = query.Where(r => r.PricePerNight >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(r => r.PricePerNight <= filter.MaxPrice.Value);
            }

            // Filtrar por capacidad
            if (filter.MinCapacity.HasValue)
            {
                query = query.Where(r => r.Capacity >= filter.MinCapacity.Value);
            }

            int totalCount = await query.CountAsync();

            // Ordenar
            query = filter.SortDescending
                ? query.OrderByDescending(r => EF.Property<object>(r, filter.SortBy ?? "RoomNumber"))
                : query.OrderBy(r => EF.Property<object>(r, filter.SortBy ?? "RoomNumber"));

            // Paginar
            var rooms = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (rooms, totalCount);
        }
    }
}
