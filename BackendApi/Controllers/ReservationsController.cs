// Controllers/ReservationsController.cs
using BackendApi.Data;
using BackendApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ReservationsController(AppDbContext db) => _db = db;

        // -----------------------
        // DTOs (response + create)
        // -----------------------
        private sealed class ReservationDto
        {
            public int ReservationId { get; set; }
            public int UserId { get; set; }
            public int RoomId { get; set; }
            public DateTime CheckInDate { get; set; }
            public DateTime CheckOutDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string Status { get; set; } = string.Empty;
        }

        // Make this PUBLIC to fix the accessibility issue
        public sealed class CreateReservationRequest
        {
            public int UserId { get; set; }
            public int RoomId { get; set; }
            public DateTime CheckInDate { get; set; }   // expects ISO date/time
            public DateTime CheckOutDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string? Status { get; set; }         // optional; default "reserved"
        }

        // -----------
        // GET: All
        // -----------
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.Reservations.AsNoTracking()
                .Select(r => new ReservationDto
                {
                    ReservationId = r.ReservationId,
                    UserId = r.UserId,
                    RoomId = r.RoomId,
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                    TotalAmount = r.TotalAmount,
                    Status = r.Status
                })
                .ToListAsync();

            return Ok(items);
        }

        // ---------------
        // GET: By Id
        // ---------------
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _db.Reservations.AsNoTracking()
                .Where(r => r.ReservationId == id)
                .Select(r => new ReservationDto
                {
                    ReservationId = r.ReservationId,
                    UserId = r.UserId,
                    RoomId = r.RoomId,
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                    TotalAmount = r.TotalAmount,
                    Status = r.Status
                })
                .FirstOrDefaultAsync();

            return item is null ? NotFound() : Ok(item);
        }

        // --------------------------
        // POST: Create Reservation
        // --------------------------
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReservationRequest req)
        {
            // Basic validation
            if (req.UserId <= 0 || req.RoomId <= 0)
                return BadRequest(new { message = "user_id and room_id are required and must be positive." });

            if (req.CheckInDate.Date >= req.CheckOutDate.Date)
                return BadRequest(new { message = "check_in_date must be before check_out_date." });

            if (req.TotalAmount < 0)
                return BadRequest(new { message = "total_amount must be >= 0." });

            // Validate user exists and is a guest
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == req.UserId);
            if (user is null)
                return NotFound(new { message = "User not found." });

            if (!string.Equals(user.Role?.Trim(), "guest", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { message = "Only users with role 'guest' can create reservations." });

            // Validate room exists
            var room = await _db.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.RoomId == req.RoomId);
            if (room is null)
                return NotFound(new { message = "Room not found." });

            // Default status to 'reserved' if not provided
            var status = string.IsNullOrWhiteSpace(req.Status) ? "reserved" : req.Status!.Trim();

            var entity = new Reservation
            {
                UserId = req.UserId,
                RoomId = req.RoomId,
                CheckInDate = req.CheckInDate.Date,   // store as date-only semantics if your column is DATE
                CheckOutDate = req.CheckOutDate.Date,
                TotalAmount = req.TotalAmount,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Reservations.Add(entity);
            await _db.SaveChangesAsync();

            var payload = new ReservationDto
            {
                ReservationId = entity.ReservationId,
                UserId = entity.UserId,
                RoomId = entity.RoomId,
                CheckInDate = entity.CheckInDate,
                CheckOutDate = entity.CheckOutDate,
                TotalAmount = entity.TotalAmount,
                Status = entity.Status
            };

            // Uses GetById for Location header
            return CreatedAtAction(nameof(GetById), new { id = entity.ReservationId }, payload);
        }
    }
}
