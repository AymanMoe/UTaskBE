namespace UTask.Models
{
    public class ProviderCategory
    {
        public int ProviderId { get; set; }
        public int CategoryId { get; set; }
        public Provider Provider { get; set; }
        public Category Category { get; set; }
    }
}
