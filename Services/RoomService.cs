using Microsoft.EntityFrameworkCore;
using ResortTralaleritos.Data;
using ResortTralaleritos.Models;

namespace ResortTralaleritos.Services
{
    /// <summary>
    /// Servicio para gestionar operaciones de habitaciones incluyendo:
    /// - Búsqueda con filtros avanzados (CU-HABITACION-03)
    /// - Auditoría de cambios
    /// - Validaciones de datos
    /// </summary>
    public interface IRoomService
    {
        Task<(List<Room> rooms, int totalCount)> SearchRoomsAsync(RoomFilterDto filter);
        Task<Room?> GetRoomByIdAsync(int roomId);
        Task<Room> UpdateRoomAsync(int roomId, Room room, string modifiedBy, string? ipAddress = null);
        Task<Room> CreateRoomAsync(Room room, string createdBy);
        Task LogAuditAsync(int roomId, string operation, string modifiedBy, string? fieldName = null, 
            string? oldValue = null, string? newValue = null, string? changeDescription = null, string? ipAddress = null);
        Task<List<RoomAuditLog>> GetRoomAuditHistoryAsync(int roomId);
    }

    public class RoomService : IRoomService
    {
        private readonly AppDbContext _context;

        public RoomService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Busca habitaciones aplicando filtros avanzados
        /// </summary>
        public async Task<(List<Room> rooms, int totalCount)> SearchRoomsAsync(RoomFilterDto filter)
        {
            var query = _context.Rooms.AsQueryable();

            // Aplicar filtro por número de habitación
            if (!string.IsNullOrWhiteSpace(filter.RoomNumber))
            {
                query = query.Where(r => r.RoomNumber.Contains(filter.RoomNumber));
            }

            // Aplicar filtro por tipo de habitación
            if (!string.IsNullOrWhiteSpace(filter.RoomType))
            {
                query = query.Where(r => r.RoomType == filter.RoomType);
            }

            // Aplicar filtro por estado
            if (filter.Status.HasValue)
            {
                query = query.Where(r => r.Status == filter.Status.Value);
            }

            // Aplicar filtro por rango de precio
            if (filter.MinPrice.HasValue)
            {
                query = query.Where(r => r.PricePerNight >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(r => r.PricePerNight <= filter.MaxPrice.Value);
            }

            // Aplicar filtro por capacidad mínima
            if (filter.MinCapacity.HasValue)
            {
                query = query.Where(r => r.Capacity >= filter.MinCapacity.Value);
            }

            // Aplicar filtro por disponibilidad
            if (filter.IsAvailable.HasValue)
            {
                query = query.Where(r => r.IsAvailable == filter.IsAvailable.Value);
            }

            // Obtener el total de registros antes de paginar
            var totalCount = await query.CountAsync();

            // Aplicar ordenamiento
            query = filter.SortBy?.ToLower() switch
            {
                "price" or "pricepernighT" => filter.SortDescending 
                    ? query.OrderByDescending(r => r.PricePerNight) 
                    : query.OrderBy(r => r.PricePerNight),
                
                "capacity" => filter.SortDescending 
                    ? query.OrderByDescending(r => r.Capacity) 
                    : query.OrderBy(r => r.Capacity),
                
                _ => filter.SortDescending 
                    ? query.OrderByDescending(r => r.RoomNumber) 
                    : query.OrderBy(r => r.RoomNumber)
            };

            // Aplicar paginación
            var rooms = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (rooms, totalCount);
        }

        /// <summary>
        /// Obtiene una habitación por su ID
        /// </summary>
        public async Task<Room?> GetRoomByIdAsync(int roomId)
        {
            return await _context.Rooms
                .Include(r => r.AuditLogs)
                .FirstOrDefaultAsync(r => r.RoomId == roomId);
        }

        /// <summary>
        /// Actualiza los datos de una habitación con auditoría
        /// </summary>
        public async Task<Room> UpdateRoomAsync(int roomId, Room updatedRoom, string modifiedBy, string? ipAddress = null)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
            {
                throw new ArgumentException($"Habitación con ID {roomId} no encontrada");
            }

            // Validar datos
            ValidateRoomData(updatedRoom);

            // Registrar cambios en auditoría
            if (room.RoomNumber != updatedRoom.RoomNumber)
            {
                await LogAuditAsync(roomId, "Update", modifiedBy, nameof(Room.RoomNumber), 
                    room.RoomNumber, updatedRoom.RoomNumber, ipAddress: ipAddress);
            }

            if (room.RoomType != updatedRoom.RoomType)
            {
                await LogAuditAsync(roomId, "Update", modifiedBy, nameof(Room.RoomType), 
                    room.RoomType, updatedRoom.RoomType, ipAddress: ipAddress);
            }

            if (room.PricePerNight != updatedRoom.PricePerNight)
            {
                await LogAuditAsync(roomId, "Update", modifiedBy, nameof(Room.PricePerNight), 
                    room.PricePerNight.ToString(), updatedRoom.PricePerNight.ToString(), ipAddress: ipAddress);
            }

            if (room.Status != updatedRoom.Status)
            {
                await LogAuditAsync(roomId, "Update", modifiedBy, nameof(Room.Status), 
                    room.Status.ToString(), updatedRoom.Status.ToString(), ipAddress: ipAddress);
            }

            if (room.Capacity != updatedRoom.Capacity || room.Beds != updatedRoom.Beds)
            {
                await LogAuditAsync(roomId, "Update", modifiedBy, "Capacidad y Camas", 
                    $"Cap: {room.Capacity}, Camas: {room.Beds}", 
                    $"Cap: {updatedRoom.Capacity}, Camas: {updatedRoom.Beds}", 
                    ipAddress: ipAddress);
            }

            if (room.Description != updatedRoom.Description)
            {
                await LogAuditAsync(roomId, "Update", modifiedBy, nameof(Room.Description), 
                    room.Description, updatedRoom.Description, ipAddress: ipAddress);
            }

            // Actualizar la habitación
            room.RoomNumber = updatedRoom.RoomNumber;
            room.RoomType = updatedRoom.RoomType;
            room.Description = updatedRoom.Description;
            room.Capacity = updatedRoom.Capacity;
            room.Beds = updatedRoom.Beds;
            room.PricePerNight = updatedRoom.PricePerNight;
            room.IsAvailable = updatedRoom.IsAvailable;
            room.Status = updatedRoom.Status;
            room.UpdatedAt = DateTime.Now;
            room.ModifiedBy = modifiedBy;

            _context.Update(room);
            await _context.SaveChangesAsync();

            return room;
        }

        /// <summary>
        /// Crea una nueva habitación con auditoría
        /// </summary>
        public async Task<Room> CreateRoomAsync(Room room, string createdBy)
        {
            ValidateRoomData(room);

            room.CreatedBy = createdBy;
            room.CreatedAt = DateTime.Now;

            _context.Add(room);
            await _context.SaveChangesAsync();

            // Registrar creación en auditoría
            await LogAuditAsync(room.RoomId, "Create", createdBy, 
                changeDescription: $"Habitación {room.RoomNumber} creada");

            return room;
        }

        /// <summary>
        /// Registra un cambio en la auditoría de la habitación
        /// </summary>
        public async Task LogAuditAsync(int roomId, string operation, string modifiedBy, 
            string? fieldName = null, string? oldValue = null, string? newValue = null, 
            string? changeDescription = null, string? ipAddress = null)
        {
            var auditLog = new RoomAuditLog
            {
                RoomId = roomId,
                ModifiedBy = modifiedBy,
                ModifiedDate = DateTime.Now,
                Operation = operation,
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValue,
                ChangeDescription = changeDescription,
                IpAddress = ipAddress
            };

            _context.RoomAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Obtiene el histórico de auditoría de una habitación
        /// </summary>
        public async Task<List<RoomAuditLog>> GetRoomAuditHistoryAsync(int roomId)
        {
            return await _context.RoomAuditLogs
                .Where(a => a.RoomId == roomId)
                .OrderByDescending(a => a.ModifiedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Valida que los datos de la habitación sean correctos
        /// </summary>
        private void ValidateRoomData(Room room)
        {
            if (string.IsNullOrWhiteSpace(room.RoomNumber))
                throw new ArgumentException("El número de habitación es requerido");

            if (string.IsNullOrWhiteSpace(room.RoomType))
                throw new ArgumentException("El tipo de habitación es requerido");

            if (room.PricePerNight < 0)
                throw new ArgumentException("El precio no puede ser negativo");

            if (room.Capacity < 1 || room.Capacity > 10)
                throw new ArgumentException("La capacidad debe estar entre 1 y 10");

            if (room.Beds < 0 || room.Beds > 10)
                throw new ArgumentException("El número de camas debe estar entre 0 y 10");

            // Validar que el tipo de habitación existe
            var validTypes = _context.RoomTypes.Select(rt => rt.Name).ToList();
            if (!validTypes.Contains(room.RoomType))
            {
                throw new ArgumentException($"El tipo de habitación '{room.RoomType}' no es válido");
            }
        }
    }
}
