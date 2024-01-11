using System.ComponentModel.DataAnnotations.Schema;

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
        public Booking? Booking { get; set; }
        public ICollection<ProviderCategory> Providers { get; set; }
    }
}