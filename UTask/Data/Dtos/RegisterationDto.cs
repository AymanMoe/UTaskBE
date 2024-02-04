namespace UTask.Data.Dtos
{
    public class RegisterationDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Type { get; set; }
        public AddressDto Address { get; set; }
        public List<int>? Categories { get; set; }
    }
}
