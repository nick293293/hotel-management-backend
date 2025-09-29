using BackendApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PingController : ControllerBase
    {
        private readonly AppDbContext _db;
        public PingController(AppDbContext db) => _db = db;

        // GET: /api/ping
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var rows = await _db.Ping
                .OrderByDescending(p => p.Id)
                .Take(20)
                .ToListAsync();
            return Ok(rows);
        }
    }
}
