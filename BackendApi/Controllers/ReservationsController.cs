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

        // GET: /api/reservations
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reservations = await _db.Reservations
                .AsNoTracking()
                .Include(x => x.User) // optional: remove if you don't want full user payload
                .Include(x => x.Room) // optional
                .ToListAsync();

            return Ok(reservations);
        }

        // GET: /api/reservations/1001
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reservation = await _db.Reservations
                .AsNoTracking()
                .Include(x => x.User) // optional
                .Include(x => x.Room) // optional
                .FirstOrDefaultAsync(x => x.ReservationId == id);

            return reservation is null ? NotFound() : Ok(reservation);
        }
    }
}
