namespace UTask.Data.Dtos
{
    public class ReviewDto
    {
        public string Comment { get; set; }
        public double Rating { get; set; }
        public DateTime ReviewDate { get; set; }
        public int ProviderId { get; set; }
        public int BookingId { get; set; }
    }
}
