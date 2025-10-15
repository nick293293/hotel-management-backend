using BackendApi.Data;
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
