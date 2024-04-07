using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace UTask.Models
{
    public class Booking
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime? ServiceDate { get; set; }
        public DateTime? BookingDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Status { get; set; }
        public int? ProviderId { get; set; }
        public int? ClientId { get; set; }
        public int? CategoryId { get; set; }
        public string? Notes { get; set; }
        public int? AddressId { get; set; }

        //Navigation properties
        [JsonIgnore]
        public ICollection<NotifiedProvider> NotifiedProviders { get; set; }
        [JsonIgnore]
        public Provider Provider { get; set; }
        [JsonIgnore]
        public Client Client { get; set; }
        [JsonIgnore]
        public Category Category { get; set; }
        [JsonIgnore]
        public Address Address { get; set; }
        [JsonIgnore]
        public Review Review { get; set; }


    }
}
