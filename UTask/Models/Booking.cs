using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace UTask.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public DateTime? ServiceDate { get; set; }
        public DateTime? BookingDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Status { get; set; }
        public int ProviderId { get; set; }
        public int ClientId { get; set; }
        public int CategoryId { get; set; }

        public Provider Provider { get; set; }
        public Client Client { get; set; }
        public Category Category { get; set; }


    }
}
