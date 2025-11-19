using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BackendApi.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int ReservationId { get; set; }

        [ForeignKey("ReservationId")]
        [JsonIgnore]
        public Reservation? Reservation { get; set; } // ✅ fixed

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        [RegularExpression("pending|processed|refunded")]
        public string Status { get; set; } = "pending";
    }
}
