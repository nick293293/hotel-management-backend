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

        // sanity check: no DB touch
        [HttpGet("ping")]
        public IActionResult Ping() => Ok(new { ok = true });

        // DB reachability + show actual exception text
        [HttpGet("db")]
        public async Task<IActionResult> Db()
        {
            try
            {
                var ok = await _db.Database.CanConnectAsync();
                return ok ? Ok(new { ok = true }) : StatusCode(500, new { ok = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ok = false, error = ex.Message });
            }
        }
    }
}
