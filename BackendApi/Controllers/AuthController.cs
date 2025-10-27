using BackendApi.Data;
using BackendApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AuthController(AppDbContext db) => _db = db;

        // -------- Helpers --------
        private static IActionResult MissingFields() =>
            new BadRequestObjectResult(new LoginResponse { Message = "Email and password are required." });

        private static LoginResponse OkPayload(BackendApi.Models.User u) => new()
        {
            Message = "Login successful.",
            UserId = u.UserId,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Role = u.Role
        };

        // -------- Staff login --------
        // POST: /api/auth/staff-login
        [HttpPost("staff-login")]
        public async Task<IActionResult> StaffLogin([FromBody] LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return MissingFields();

            var user = await _db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == req.Email.Trim());
            if (user is null)
                return NotFound(new LoginResponse { Message = "User not found." });

            var passwordOk = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHashed);
            if (!passwordOk)
                return Unauthorized(new LoginResponse { Message = "Unauthorized: invalid credentials." });

            if (!string.Equals(user.Role?.Trim(), "staff", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new LoginResponse { Message = "Unauthorized: role is not staff." });

            return Ok(OkPayload(user));
        }

        // -------- Guest login --------
        // POST: /api/auth/guest-login
        [HttpPost("guest-login")]
        public async Task<IActionResult> GuestLogin([FromBody] LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return MissingFields();

            var user = await _db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == req.Email.Trim());
            if (user is null)
                return NotFound(new LoginResponse { Message = "User not found." });

            var passwordOk = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHashed);
            if (!passwordOk)
                return Unauthorized(new LoginResponse { Message = "Unauthorized: invalid credentials." });

            if (!string.Equals(user.Role?.Trim(), "guest", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new LoginResponse { Message = "Unauthorized: role is not guest." });

            return Ok(OkPayload(user));
        }

        // -------- Guest sign-up --------
        // POST: /api/auth/guest-signup
        [HttpPost("guest-signup")]
        public async Task<IActionResult> GuestSignup([FromBody] GuestSignupRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) ||
                string.IsNullOrWhiteSpace(req.Password) ||
                string.IsNullOrWhiteSpace(req.FirstName) ||
                string.IsNullOrWhiteSpace(req.LastName))
            {
                return BadRequest(new { message = "Missing required fields." });
            }

            var email = req.Email.Trim();
            var exists = await _db.Users.AsNoTracking().AnyAsync(u => u.Email == email);
            if (exists)
                return Conflict(new { message = "Email already exists." });

            var hashed = BCrypt.Net.BCrypt.HashPassword(req.Password);

            var user = new BackendApi.Models.User
            {
                Email = email,
                PasswordHashed = hashed,
                FirstName = req.FirstName.Trim(),
                LastName = req.LastName.Trim(),
                PhoneNumber = req.PhoneNumber?.Trim(),
                Address = req.Address?.Trim(),
                Role = "guest",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Guest registered successfully.",
                userId = user.UserId,
                email = user.Email,
                role = user.Role
            });
        }
    }
}
