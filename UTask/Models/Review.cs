namespace UTask.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string ReviewText { get; set; }
        public double Rating { get; set; }
        public DateTime ReviewDate { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public int ProviderId { get; set; }
        public Provider Provider { get; set; }
        public int? BookingId { get; set; }
        public Booking Booking { get; set; }
    }
}
