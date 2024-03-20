using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UTask.Models
{
    public class Category
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string ServiceName { get; set; }
        public string Division { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }
        //Navigation properties
        [JsonIgnore]
        public ICollection<ProviderCategory> Providers { get; set; }
        [JsonIgnore]
        public ICollection<Booking> Bookings { get; set; }
    }
}