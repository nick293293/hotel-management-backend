using System;

namespace BackendApi.Models
{
    public class Ping
    {
        public int Id { get; set; }
        public string Note { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
