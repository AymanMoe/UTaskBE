using Newtonsoft.Json;
using NuGet.Protocol.Plugins;
using System.ComponentModel.DataAnnotations.Schema;

namespace UTask.Models
{
    public class Provider
    {
        // This is the primary key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // Unique identifier for the provider
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get;  set; }
        public string Image { get;  set; }
        public string Phone { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Navigation properties
        public string AppUserName { get; set; }
        public AppUser? AppUser  { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<ProviderCategory> Categories { get; set; }
        
    }
}
