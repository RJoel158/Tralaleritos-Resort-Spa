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
            var query = _context.Rooms.Include(r => r.RoomType).AsQueryable();

            // Aplicar filtro por número de habitación
            if (!string.IsNullOrWhiteSpace(filter.RoomNumber))
            {
                query = query.Where(r => r.RoomNumber.Contains(filter.RoomNumber));
            }

            // Aplicar filtro por tipo de habitación
            if (!string.IsNullOrWhiteSpace(filter.RoomType))
            {
                query = query.Where(r => r.RoomType != null && r.RoomType.Name.Contains(filter.RoomType));
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

            // Aplicar filtro por capacidad mínima (usando RoomType.DefaultCapacity)
            if (filter.MinCapacity.HasValue)
            {
                query = query.Where(r => r.RoomType != null && r.RoomType.DefaultCapacity >= filter.MinCapacity.Value);
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
                    ? query.OrderByDescending(r => r.RoomType != null ? r.RoomType.DefaultCapacity : 0) 
                    : query.OrderBy(r => r.RoomType != null ? r.RoomType.DefaultCapacity : 0),
                
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
                .Include(r => r.RoomType)
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

            if (room.RoomTypeId != updatedRoom.RoomTypeId)
            {
                await LogAuditAsync(roomId, "Update", modifiedBy, nameof(Room.RoomTypeId), 
                    room.RoomTypeId.ToString(), updatedRoom.RoomTypeId.ToString(), ipAddress: ipAddress);
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

            if (room.Description != updatedRoom.Description)
            {
                await LogAuditAsync(roomId, "Update", modifiedBy, nameof(Room.Description), 
                    room.Description, updatedRoom.Description, ipAddress: ipAddress);
            }

            // Actualizar la habitación
            room.RoomNumber = updatedRoom.RoomNumber;
            room.RoomTypeId = updatedRoom.RoomTypeId;
            room.Description = updatedRoom.Description;
            room.PricePerNight = updatedRoom.PricePerNight;
            room.Status = updatedRoom.Status;
            room.UpdateDate = DateTime.Now;

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

            room.RegistrationDate = DateTime.Now;

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

            if (room.RoomTypeId <= 0)
                throw new ArgumentException("El tipo de habitación es requerido");

            if (room.PricePerNight < 0)
                throw new ArgumentException("El precio no puede ser negativo");

            // Validar que el tipo de habitación existe
            var roomTypeExists = _context.RoomTypes.Any(rt => rt.RoomTypeId == room.RoomTypeId);
            if (!roomTypeExists)
            {
                throw new ArgumentException($"El tipo de habitación con ID '{room.RoomTypeId}' no es válido");
            }
        }
    }
}
