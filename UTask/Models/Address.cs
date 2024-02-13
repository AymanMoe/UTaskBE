using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace UTask.Models
{
    public class Address
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AddressId { get; set; }
        public string StreetAddress { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Province { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string? Apartment { get; set; }
        public string? Building { get; set; }
        public string? Floor { get; set; }
        public string? Intercom { get; set; }
        public string? Notes { get; set; }

        public string AppUserName { get; set; }
        [JsonIgnore]
        public AppUser AppUser { get; set; } = null!;
       
        

    }
}
