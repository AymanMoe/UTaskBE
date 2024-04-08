using System.ComponentModel.DataAnnotations.Schema;

namespace UTask.Models
{
    public class Invoice
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime InvoiceDate { get; set; }
        public double? Amount { get; set; }
        public string Status { get; set; }
        public int? BookingId { get; set; }
        public Booking Booking { get; set; }
        public int? CategoryId { get; set; }
        public Category Category { get; set; }
        public int? ProviderId { get; set; }
        public Provider Provider { get; set; }
        public int? ClientId { get; set; }
        public Client Client { get; set; }

    }
}
