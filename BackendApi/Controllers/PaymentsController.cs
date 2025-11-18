using BackendApi.Data;
using BackendApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ GET: api/payments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPayments()
        {
            var payments = await _context.Payments
                .AsNoTracking()
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return Ok(payments);
        }

        // ✅ GET: api/payments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPaymentById(int id)
        {
            var payment = await _context.Payments.FindAsync(id);

            if (payment == null)
                return NotFound(new { message = "Payment not found" });

            return Ok(payment);
        }

        // ✅ POST: api/payments
        [HttpPost]
        public async Task<ActionResult<Payment>> CreatePayment([FromBody] Payment newPayment)
        {
            // Validation: Check reservation exists
            var reservationExists = await _context.Reservations
                .AnyAsync(r => r.ReservationId == newPayment.ReservationId);

            if (!reservationExists)
                return BadRequest(new { message = "Invalid ReservationId" });

            // Default values
            newPayment.PaymentDate = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(newPayment.Status))
                newPayment.Status = "pending";

            _context.Payments.Add(newPayment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPaymentById),
                new { id = newPayment.PaymentId },
                newPayment);
        }
    }
}
