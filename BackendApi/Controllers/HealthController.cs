using System.Data;
using BackendApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly AppDbContext _db;
        public HealthController(AppDbContext db) => _db = db;

        // Sanity: app up?
        [HttpGet("ping")]
        public IActionResult Ping() => Ok(new { ok = true });

        [HttpGet("cs")]
        public IActionResult Cs([FromServices] IConfiguration cfg)
        {
            var raw = cfg.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(raw)) return Ok(new { hasValue = false });

            // Parse without exposing password
            var b = new MySqlConnector.MySqlConnectionStringBuilder(raw);
            return Ok(new { hasValue = true, server = b.Server, port = b.Port, database = b.Database, user = b.UserID });
        }


        // Can EF open the DB?
        [HttpGet("db")]
        public async Task<IActionResult> Db()
        {
            var ok = await _db.Database.CanConnectAsync();
            return ok ? Ok(new { ok = true }) : StatusCode(500, new { ok = false });
        }

        // Echo CURRENT_USER() and DATABASE() to confirm creds/schema in prod
        [HttpGet("whoami")]
        public async Task<IActionResult> WhoAmI()
        {
            await using var conn = _db.Database.GetDbConnection();
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT CURRENT_USER() AS user, DATABASE() AS db";
            await using var rdr = await cmd.ExecuteReaderAsync();
            if (await rdr.ReadAsync())
                return Ok(new { user = rdr["user"]?.ToString(), db = rdr["db"]?.ToString() });
            return Ok(new { user = "", db = "" });
        }

        // List columns in users table (to catch password vs password_hashed)
        [HttpGet("users-columns")]
        public async Task<IActionResult> UsersColumns()
        {
            await using var conn = _db.Database.GetDbConnection();
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT column_name
                FROM information_schema.columns
                WHERE table_schema = DATABASE() AND table_name = 'users'
                ORDER BY ordinal_position";
            var cols = new List<string>();
            await using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
                cols.Add(rdr.GetString(0));
            return Ok(new { columns = cols });
        }
    }
}
