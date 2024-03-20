namespace UTask.Models
{
    public class NotifiedProvider
    {
        public int Id { get; set; }
        public int ProviderId { get; set; }
        public int BookingId { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public DateTime? NotifiedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Provider Provider { get; set; }
        public Booking Booking { get; set; }
    }
}
