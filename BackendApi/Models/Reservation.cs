namespace BackendApi.Models
{
    public class Reservation
    {
        public int ReservationId { get; set; }
        public int UserId { get; set; }
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "reserved"; // 'reserved','checkedin','checkedout','cancelled'
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // nav
        public User? User { get; set; }
        public Room? Room { get; set; }
    }
}
