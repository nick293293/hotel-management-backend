using BackendApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public RoomsController(AppDbContext db) => _db = db;

        // GET: /api/rooms
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var rooms = await _db.Rooms.AsNoTracking().ToListAsync();
            return Ok(rooms);
        }

        // GET: /api/rooms/12
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var room = await _db.Rooms.AsNoTracking().FirstOrDefaultAsync(x => x.RoomId == id);
            return room is null ? NotFound() : Ok(room);
        }
    }
}
