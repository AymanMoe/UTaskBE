namespace UTask.Data.Dtos
{
    public class BookingDto
    {
        public string ServiceDay { get; set; }
        public string ServiceTime { get; set; }
        public DateTime? BookingDate { get; set; }
        protected DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending";
        public int CategoryId { get; set; }
        public string? Notes { get; set; }
        public AddressDto? Address { get; set; } = null!;

    }
}
