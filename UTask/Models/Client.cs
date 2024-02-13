using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
namespace UTask.Models
{
    public class Client
    {

        // This is the primary key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        
        public string Phone { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Navigation properties
       
        public string AppUserName { get; set; }
        public AppUser? AppUser { get; set; }
        public IEnumerable<Booking> Bookings { get; set; }

    }
}
