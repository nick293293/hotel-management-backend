using System.ComponentModel.DataAnnotations;

namespace BackendApi.Models
{
    public class CreateReservationRequest
    {
        [Required] public int UserId { get; set; }
        [Required] public int RoomId { get; set; }
        [Required] public DateOnly CheckInDate { get; set; }
        [Required] public DateOnly CheckOutDate { get; set; }
        [Range(0, double.MaxValue)] public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
    }
}
