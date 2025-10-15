using BackendApi.Data;
using Microsoft.AspNetCore.Mvc;

namespace BackendApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly AppDbContext _db;
        public HealthController(AppDbContext db) => _db = db;

        [HttpGet("db")]
        public async Task<IActionResult> Db()
        {
            try
            {
                var can = await _db.Database.CanConnectAsync();
                return can ? Ok(new { ok = true }) : StatusCode(500, new { ok = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ok = false, error = ex.Message });
            }
        }
    }
}
