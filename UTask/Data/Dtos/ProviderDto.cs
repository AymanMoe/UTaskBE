namespace UTask.Data.Dtos
{
    public class ProviderDto
    {
        public string Name { get; set; }
        public string Bio { get; set; }
        public string DateJoined { get; set; }
        public string City { get; set; }
        public string Rating { get; set; }
        public string Image { get; set; }
        public List<CategoryDtos> Services { get; set; }
    }
}
