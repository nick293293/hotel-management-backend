using System.Text.Json.Serialization;

namespace BackendApi.Models
{
    public class Room
    {
        public int RoomId { get; set; }
        public string RoomType { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = "available";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // 👇 prevents cycles: Room -> Reservations -> Room -> ...
        [JsonIgnore]
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
